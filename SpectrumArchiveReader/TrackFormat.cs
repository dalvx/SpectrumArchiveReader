using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SpectrumArchiveReader
{
    public class TrackFormat : ICloneable
    {
        public static TrackFormat TrDos = new TrackFormat(TrackFormatName.TrDosSequential);
        public static TrackFormat Cpm = new TrackFormat(TrackFormatName.CpmSequential);
        public static TrackFormat IsDos = new TrackFormat(TrackFormatName.IsDosSequential);

        /// <summary>
        /// Расположение секторов.
        /// </summary>
        public MList<SectorInfo> Layout;
        /// <summary>
        /// Номер трека с которого было взята схема расположения секторов.
        /// </summary>
        public int LayoutTrack;
        /// <summary>
        /// Индекс длинного сектора (с временем более 19 мс). Если такого сектора нет, то Int32.MinValue.
        /// </summary>
        public int LongSectorIndex;
        /// <summary>
        /// Таймер отсчитывющий время с момента последней синхронизации с концом сектора.
        /// </summary>
        public Timer Timer = new Timer();
        /// <summary>
        /// Индекс сектора с концом которого была сделана последняя синхронизация.
        /// </summary>
        private int syncSectorIndex;
        /// <summary>
        /// Номер трека на котором была сделана последняя синхронизация.
        /// </summary>
        private int syncTrack;
        public double SpinTime;
        public TrackFormatName FormatName = TrackFormatName.NoHeaders;
        /// <summary>
        /// Флаг наличия синхронизации с сектором. Синхронизация истекает через 3 секунды.
        /// </summary>
        public bool IsSync { get { return Timer.IsRunning && Timer.Elapsed.Seconds < 3; } }
        public const double BytesPerMs = 31.25;

        public TrackFormat(int sectorCnt)
        {
            Layout = new MList<SectorInfo>(sectorCnt);
        }

        public TrackFormat(TrackFormatName formatName)
        {
            Layout = new MList<SectorInfo>(16);
            SetFormat(formatName);
        }

        private int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public void Sync(int track, int sector)
        {
            Timer.Restart();
            syncTrack = track;
            for (int i = 0; i < Layout.Cnt; i++)
            {
                if (Layout.Data[i].SectorNumber == sector)
                {
                    syncSectorIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Синхронизация по концу заголовка сектора.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="sector"></param>
        public void SyncByHeader(int track, int sector)
        {
            syncTrack = track;
            for (int i = 0; i < Layout.Cnt; i++)
            {
                if (Layout.Data[i].SectorNumber == sector)
                {
                    syncSectorIndex = i;
                    // Устанавливаем время отрицательным. Оно должно быть равно времени между концом заголовка и концом сектора (со знаком минус).
                    // Время должно стать нулевым в момент прохода головки по концу сектора.
                    Timer.Restart((-Layout.Data[i].SizeBytes + 22 + 12) / BytesPerMs);   // 22, 12 - пробелы после заголовка в байтах
                    return;
                }
            }
        }

        /// <summary>
        /// Синхронизация сектора с другим объектом TrackFormat.
        /// </summary>
        /// <param name="trackFormat"></param>
        public void Sync(TrackFormat trackFormat)
        {
            if (trackFormat == null || trackFormat.Layout.Cnt == 0) return;
            int sectorNum = trackFormat.Layout.Data[trackFormat.syncSectorIndex].SectorNumber;
            int newSyncIndex = FindSectorIndex(sectorNum);
            if (newSyncIndex < 0) return;
            Timer.Assign(trackFormat.Timer);
            syncSectorIndex = newSyncIndex;
            syncTrack = trackFormat.syncTrack;
        }

        /// <summary>
        /// Получение ближайшего к головке дисковода сектора в соответствии с моделью вращения диска.
        /// </summary>
        /// <param name="track">Трек</param>
        /// <param name="waitTimeMs">Время которое надо пропустить.</param>
        /// <param name="skip">Количество секторов которые надо пропустить.</param>
        /// <param name="timeAfterSync">Время которое должно пройти с момента последней синхронизации до конца возвращенного сектора.</param>
        /// <returns></returns>
        public SectorInfo GetClosestSector(int track, double waitTimeMs, int skip, out double timeAfterSync)
        {
            timeAfterSync = 0;
            if (!IsSync) return Layout.Data[syncSectorIndex];
            double time0 = Timer.ElapsedMs + waitTimeMs;
            double time = time0 > 0 ? time0 % SpinTime : time0;
            int index = (syncSectorIndex + 1) % Layout.Cnt;

            int newLongSectorIndex = LongSectorIndex;
            if (FormatName == TrackFormatName.TrDos5_04T)
            {
                // Вычисление смещения длинного сектора для формата TR-DOS 5.04T относительно трека для которого была определена схема расположения секторов.
                int diff = -2 * (track - LayoutTrack);
                newLongSectorIndex = Mod(LongSectorIndex + diff, Layout.Cnt);

                // Вычисление смещения точки синхронизации.
                int diff0 = -2 * (track - syncTrack);
                index = Mod(index + diff0, Layout.Cnt);
            }

            for (int i = 0; i < Layout.Cnt * 10; i++)
            {
                int x = index;
                if (FormatName == TrackFormatName.TrDos5_04T)
                {
                    // Для формата TR-DOS 5.04T здесь происходит подмена секторов налету если попали на длинный сектор, имитируя его смещение.
                    // Всё делается в расчете на то что все короткие сектора имеют одинаковую длительность порядка 12 мс, и длинный сектор только один.

                    if (index == newLongSectorIndex)
                    {
                        x = LongSectorIndex;    // newLongSectorIndex подменяем на LongSectorIndex, делая его длинным.
                    }
                    else if (index == LongSectorIndex)
                    {
                        x = newLongSectorIndex; // Берем короткий сектор (или длинный, если индексы совпадают) если оказались на старом длинном.
                    }
                }
                timeAfterSync += Layout.Data[x].TimeMs;
                if (time < Layout.Data[x].TimeMs && time <= Layout.Data[x].GetGapTimeSpan())
                {
                    if (skip == 0)
                    {
                        SectorInfo r = Layout.Data[index];
                        r.TimeMs = Layout.Data[x].TimeMs;
                        return r;
                    }
                    skip--;
                }
                time -= Layout.Data[x].TimeMs;
                index = (index + 1) % Layout.Cnt;
            }
            throw new Exception();
        }

        public double GetSpinTime()
        {
            double sum = 0;
            for (int i = 0; i < Layout.Cnt; i++)
            {
                sum += Layout.Data[i].TimeMs;
            }
            return sum;
        }

        public void Assign(TrackFormat source)
        {
            Layout.CopyArray(source.Layout);
            Timer.Assign(source.Timer);
            LayoutTrack = source.LayoutTrack;
            LongSectorIndex = source.LongSectorIndex;
            syncSectorIndex = source.syncSectorIndex;
            syncTrack = source.syncTrack;
            SpinTime = source.SpinTime;
            FormatName = source.FormatName;
        }

        public unsafe void AcceptLayout(int track, SectorInfo* layout, int cnt, int syncSectorNumber)
        {
            int minSectorIndex = 0;
            int minSectorNumber = int.MaxValue;
            for (int i = 0; i < cnt; i++)
            {
                if (layout[i].SectorNumber < minSectorNumber)
                {
                    minSectorNumber = layout[i].SectorNumber;
                    minSectorIndex = i;
                }
            }
            Layout.EnsureCapacity(cnt);
            Layout.Cnt = cnt;
            for (int i = 0, j = minSectorIndex; i < cnt; i++)
            {
                Layout.Data[i] = layout[j];
                if (Layout.Data[i].SectorNumber == syncSectorNumber) SyncByHeader(track, syncSectorNumber);
                j++;
                if (j >= cnt) j = 0;
            }
            FormatName = GetFormatName();
            SpinTime = GetSpinTime();
            LayoutTrack = track;
        }

        public unsafe TrackFormatName GetFormatName()
        {
            if (Layout.Cnt == 0) return TrackFormatName.NoHeaders;
            if (Layout.Cnt != 16 && Layout.Cnt != 5) return TrackFormatName.Unrecognized;
            int* sectors = stackalloc int[17];
            for (int i = 0; i < 17; i++)
            {
                sectors[i] = 0;
            }
            int longSectors = 0;
            int lastLongSector = int.MinValue;
            int lastLongSectorIndex = int.MinValue;
            if (Layout.Cnt == 16)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (Layout.Data[i].SectorNumber < 1 || Layout.Data[i].SectorNumber > 16 || Layout.Data[i].SizeCode != 1) return TrackFormatName.Unrecognized;
                    if (sectors[Layout.Data[i].SectorNumber] != 0) return TrackFormatName.Unrecognized;
                    sectors[Layout.Data[i].SectorNumber] = 1;
                    if (Layout.Data[i].TimeMs > 12 + 7)     // 12 мс - обычное время чтения сектора в турбо-формате. 7 мс - время шага головки вперед.
                    {
                        longSectors++;
                        lastLongSector = Layout.Data[i].SectorNumber;
                        lastLongSectorIndex = i;
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    if (Layout.Data[i].SectorNumber != i + 1) goto next;
                }
                LongSectorIndex = lastLongSectorIndex;
                if (lastLongSector == int.MinValue) return TrackFormatName.TrDosSequential;
                if (lastLongSector == 1) return TrackFormatName.TrDosTurbo;
                return TrackFormatName.TrDos5_04T;
                next:
                for (int k = 0, i = 1, j = 9; k < 16; i++, j++, k += 2)
                {
                    if (Layout.Data[k].SectorNumber != i || Layout.Data[k + 1].SectorNumber != j) return TrackFormatName.TrDosGeneric;
                }
                return TrackFormatName.TrDosInterleave;
            }
            else if (Layout.Cnt == 5)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Layout.Data[i].SectorNumber < 1 || Layout.Data[i].SectorNumber > 5 || Layout.Data[i].SizeCode != 3) goto isdosCheck;
                    if (sectors[Layout.Data[i].SectorNumber] != 0) return TrackFormatName.Unrecognized;
                    sectors[Layout.Data[i].SectorNumber] = 1;
                }
                for (int i = 0; i < 5; i++)
                {
                    if (Layout.Data[i].SectorNumber != i + 1) goto next;
                }
                return TrackFormatName.CpmSequential;
                next:
                // Проверка на interleave.
                //for (int k = 0, i = 1, j = 9; k < 16; i++, j++, k += 2)
                //{
                //    if (Layout.Data[k].SectorNumber != i || Layout.Data[k + 1].SectorNumber != j) return TrackFormatName.TrDosGeneric;
                //}
                return TrackFormatName.CpmGeneric;
            isdosCheck:
                for (int i = 0; i < 5; i++)
                {
                    if (Layout.Data[i].SectorNumber < 1 || Layout.Data[i].SectorNumber > 9 || Layout.Data[i].SizeCode != 3) return TrackFormatName.Unrecognized;
                    if (Layout.Data[i].SectorNumber > 4 && Layout.Data[i].SectorNumber < 9) return TrackFormatName.Unrecognized;
                    if (sectors[Layout.Data[i].SectorNumber] != 0) return TrackFormatName.Unrecognized;
                    sectors[Layout.Data[i].SectorNumber] = 1;
                }
                for (int i = 0; i < 4; i++)
                {
                    if (Layout.Data[i].SectorNumber != i + 1) goto next1;
                }
                if (Layout.Data[4].SectorNumber == 9) return TrackFormatName.IsDosSequential;
                next1:
                return TrackFormatName.IsDosGeneric;
            }
            else
            {
                return TrackFormatName.Unrecognized;
            }
        }

        public void SetFormat(TrackFormatName formatName)
        {
            FormatName = formatName;
            SpinTime = 200;
            switch (formatName)
            {
                case TrackFormatName.TrDosTurbo:
                    Layout.EnsureCapacity(16);
                    Layout.Cnt = 16;
                    for (int i = 0; i < 16; i++)
                    {
                        Layout.Data[i].SizeCode = 1;
                        Layout.Data[i].SectorNumber = i + 1;
                        Layout.Data[i].TimeMs = 11.88;  // тайминги установлены неточно.
                    }
                    Layout.Data[0].TimeMs = 21.78;
                    return;

                case TrackFormatName.TrDosSequential:
                    Layout.EnsureCapacity(16);
                    Layout.Cnt = 16;
                    for (int i = 0; i < 16; i++)
                    {
                        Layout.Data[i].SizeCode = 1;
                        Layout.Data[i].SectorNumber = i + 1;
                        Layout.Data[i].TimeMs = 12.5;
                    }
                    return;

                case TrackFormatName.TrDosInterleave:
                    Layout.EnsureCapacity(16);
                    Layout.Cnt = 16;
                    for (int i = 0, j = 1, k = 9; i < 16; i += 2, j++, k++)
                    {
                        Layout.Data[i].SizeCode = 1;
                        Layout.Data[i].SectorNumber = j;
                        Layout.Data[i].TimeMs = 12.5;
                        Layout.Data[i + 1].SizeCode = 1;
                        Layout.Data[i + 1].SectorNumber = k;
                        Layout.Data[i + 1].TimeMs = 12.5;
                    }
                    return;

                case TrackFormatName.CpmSequential:
                    Layout.EnsureCapacity(5);
                    Layout.Cnt = 5;
                    for (int i = 0; i < 5; i++)
                    {
                        Layout.Data[i].SizeCode = 3;
                        Layout.Data[i].SectorNumber = i + 1;
                        Layout.Data[i].TimeMs = 40;
                    }
                    return;

                case TrackFormatName.IsDosSequential:
                    Layout.EnsureCapacity(5);
                    Layout.Cnt = 5;
                    for (int i = 0; i < 5; i++)
                    {
                        Layout.Data[i].SizeCode = 3;
                        Layout.Data[i].SectorNumber = i < 4 ? i + 1 : 9;
                        Layout.Data[i].TimeMs = 40;
                    }
                    return;

                default:
                    throw new Exception();
            }
        }

        public int FindSectorIndex(int sectorNumber)
        {
            for (int i = 0; i < Layout.Cnt; i++)
            {
                if (Layout.Data[i].SectorNumber == sectorNumber) return i;
            }
            return -1;
        }

        public bool ContainsSectorsFrom(TrackFormat trackFormat, int cylinder)
        {
            for (int i = 0; i < Layout.Cnt; i++)
            {
                int index = trackFormat.FindSectorIndex(Layout.Data[i].SectorNumber);
                if (index >= 0
                    && Layout.Data[i].SizeCode == trackFormat.Layout.Data[index].SizeCode
                    && (cylinder < 0 || Layout.Data[i].Cylinder == cylinder))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsOtherSectors(TrackFormat trackFormat, int cylinder)
        {
            for (int i = 0; i < Layout.Cnt; i++)
            {
                int index = trackFormat.FindSectorIndex(Layout.Data[i].SectorNumber);
                if ((index < 0)
                    || (cylinder >= 0 && Layout.Data[i].Cylinder != cylinder))
                    return true;
            }
            return false;
        }

        public bool DoesSatisfyFormat(TrackFormat trackFormat, int cylinder)
        {
            return ContainsSectorsFrom(trackFormat, cylinder) && !ContainsOtherSectors(trackFormat, cylinder);
        }

        public string ToStringAsSectorArray()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Layout.Cnt; i++)
            {
                sb.Append($"(c={Layout.Data[i].Cylinder}, h={Layout.Data[i].Head}, s={Layout.Data[i].SectorNumber}, {Layout.Data[i].SizeBytes}, {GP.ToString(Layout.Data[i].TimeMs, 2)} ms)");
                if (i < Layout.Cnt - 1) sb.Append(", ");
            }
            return sb.ToString();
        }

        public static int TotalSectors(MList<TrackFormat> diskFormat)
        {
            int sum = 0;
            for (int i = 0; i < diskFormat.Cnt; i++)
            {
                sum += diskFormat[i].Layout.Cnt;
            }
            return sum;
        }

        public static string ToXml(MList<TrackFormat> diskFormat)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine($"<DiskFormat TrackCount=\"{diskFormat.Cnt}\" Sectors=\"{TotalSectors(diskFormat)}\">");
            for (int i = 0; i < diskFormat.Cnt; i++)
            {
                MList<SectorInfo> layout = diskFormat[i].Layout;
                if (layout.Cnt != 0)
                {
                    sb.AppendLine($"  <Track Index=\"{i}\" SectorCount=\"{layout.Cnt}\" Name=\"{diskFormat[i].FormatName}\">");
                    for (int j = 0; j < layout.Cnt; j++)
                    {
                        sb.AppendLine($"    <Sector Cylinder=\"{layout[j].Cylinder}\" Head=\"{layout[j].Head}\" Number=\"{layout[j].SectorNumber}\" Size=\"{layout[j].SizeCode}\" Time=\"{GP.ToString(layout[j].TimeMs, 4)}\" />");
                    }
                    sb.AppendLine("  </Track>");
                }
                else
                {
                    sb.AppendLine($"  <Track Index=\"{i}\" SectorCount=\"{layout.Cnt}\" Name=\"{diskFormat[i].FormatName}\" />");
                }
            }
            sb.AppendLine("</DiskFormat>");
            return sb.ToString();
        }

        public static MList<TrackFormat> LoadXml(string fileName)
        {
            MList<TrackFormat> r = null;
            using (XmlTextReader xml = new XmlTextReader(fileName))
            {
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.EndElement) break;
                    if (xml.NodeType != XmlNodeType.Element) continue;
                    if (xml.Name == "DiskFormat")
                    {
                        r = new MList<TrackFormat>(Int32.Parse(xml.GetAttribute("TrackCount")));
                        while (xml.Read())
                        {
                            if (xml.NodeType == XmlNodeType.EndElement) break;
                            if (xml.NodeType != XmlNodeType.Element) continue;
                            switch (xml.Name)
                            {
                                case "Track":
                                    TrackFormat track = new TrackFormat(Int32.Parse(xml.GetAttribute("SectorCount")));
                                    if (!xml.IsEmptyElement)
                                    {
                                        while (xml.Read())
                                        {
                                            if (xml.NodeType == XmlNodeType.EndElement) break;
                                            if (xml.NodeType != XmlNodeType.Element) continue;
                                            switch (xml.Name)
                                            {
                                                case "Sector":
                                                    SectorInfo sector = new SectorInfo()
                                                    {
                                                        Cylinder = Int32.Parse(xml.GetAttribute("Cylinder")),
                                                        Head = Int32.Parse(xml.GetAttribute("Head")),
                                                        SectorNumber = Int32.Parse(xml.GetAttribute("Number")),
                                                        SizeCode = Int32.Parse(xml.GetAttribute("Size")),
                                                        TimeMs = Double.Parse(xml.GetAttribute("Time"), NumberStyles.Any, CultureInfo.InvariantCulture)
                                                    };
                                                    track.Layout.Add(sector);
                                                    break;
                                            }
                                        }
                                    }
                                    track.FormatName = track.GetFormatName();
                                    track.SpinTime = track.GetSpinTime();
                                    r.Add(track);
                                    break;
                            }
                        }
                    }
                }
            }
            return r;
        }

        public object Clone()
        {
            TrackFormat r = (TrackFormat)MemberwiseClone();
            r.Layout = new MList<SectorInfo>(Layout, true, false);
            r.Timer = (Timer)Timer.Clone();
            return r;
        }
    }

    public struct SectorInfo
    {
        public int Cylinder;
        public int SectorNumber;
        public int Head;
        public double TimeMs;
        public int SizeCode;

        public int SizeBytes
        {
            get
            {
                return GetSizeBytes(SizeCode);
            }
        }

        public static int GetSizeBytes(int sizeCode)
        {
            switch (sizeCode)
            {
                case 0:
                    return 128;

                case 1:
                    return 256;

                case 2:
                    return 512;

                case 3:
                    return 1024;

                case 4:
                    return 2048;

                case 5:
                    return 4096;

                case 6:
                    return 8192;

                default:
                    throw new Exception();
            }
        }

        public static int GetSizeCode(int sizeBytes)
        {
            switch (sizeBytes)
            {
                case 128:
                    return 0;

                case 256:
                    return 1;

                case 512:
                    return 2;

                case 1024:
                    return 3;

                case 2048:
                    return 4;

                case 4096:
                    return 5;

                case 8192:
                    return 6;
            }
            throw new Exception();
        }

        public double GetGapTimeSpan()
        {
            // [26.05.2020] Обнаружен формат у которого скорее всего отсутствуют пробелы в заголовке, либо очень малы.
            // Добавление одной миллисекунды также оказалось ошибочным в этом формате, т.к. не позволяет прочитать его на максимальной скорости.

            int dataAndHeaderSize = SizeBytes + 59 + 1;     // При условии пробела между заголовком и данными 22 x 4E и 12 x 00.
            double dhTime = dataAndHeaderSize / TrackFormat.BytesPerMs + 1;  // Добавляется запас 1 мс.
            return Math.Max(TimeMs - dhTime, 0);
        }

        public override string ToString()
        {
            return $"Cylinder: {Cylinder} | Head: {Head} | Sector: {SectorNumber} | Size: {SizeCode}";
        }
    }

    public enum TrackFormatName
    {
        /// <summary>
        /// В турбо-формате время между концом сектора и началом следующего составляет около 1.55 мс.
        /// Если не уложиться в это время, то следующий сектор будет пропущен.
        /// [26.05.2020] Обнаружен формат где первый сектор имеет размер 64.42 мс, а остальные в среднем 9.012 мс.
        /// Это дает размер сектора 290.6 байт (вместе с заголовком и пробелами). Отсюда следует что в заголовке нет пробелов 22 x 4E и 12 x 00.
        /// </summary>
        TrDosTurbo,
        /// <summary>
        /// Формат TR-DOS 5.04T (минус означает большой пробел в конце трека, G = 16).
        /// TRK|Layout
        /// 00: 123456789ABCDEFG-   (эквивалентно Turbo)
        /// 01: FG123456789ABCDE-
        /// 02: DEFG123456789ABC-
        /// 03: BCDEFG123456789A-
        /// 04: 9ABCDEFG12345678-
        /// 05: 789ABCDEFG123456-
        /// 06: 56789ABCDEFG1234-
        /// 07: 3456789ABCDEFG12-
        /// 08: 123456789ABCDEFG-   (эквивалентно Turbo; период 8 треков).
        /// 09: FG123456789ABCDE-
        /// 10: DEFG123456789ABC-
        /// </summary>
        TrDos5_04T,
        TrDosSequential,
        TrDosInterleave,
        TrDosGeneric,
        CpmSequential,
        CpmGeneric,
        IsDosSequential,
        IsDosGeneric,
        NoHeaders,
        Unrecognized
    }
}
