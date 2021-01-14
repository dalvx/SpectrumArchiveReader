using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace SpectrumArchiveReader
{
    public class DiskImage2
    {
        public string Name;
        public bool Modified;
        public MList<TrackFormat> Tracks = new MList<TrackFormat>(172);
        public TrackFormat this[int index] { get { return Tracks.Data[index]; } }

        public int ProcessedSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks[i] == null) continue;
                    for (int j = 0; j < Tracks[i].Layout.Cnt; j++)
                    {
                        if (Tracks[i].Layout.Data[j].ProcessResult != SectorProcessResult.Unprocessed) cnt++;
                    }
                }
                return cnt;
            }
        }

        public int UnprocessedSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks[i] == null) continue;
                    for (int j = 0; j < Tracks[i].Layout.Cnt; j++)
                    {
                        if (Tracks[i].Layout.Data[j].ProcessResult == SectorProcessResult.Unprocessed) cnt++;
                    }
                }
                return cnt;
            }
        }

        public int GoodSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks[i] == null) continue;
                    for (int j = 0; j < Tracks[i].Layout.Cnt; j++)
                    {
                        if (Tracks[i].Layout.Data[j].ProcessResult == SectorProcessResult.Good) cnt++;
                    }
                }
                return cnt;
            }
        }

        public int BadSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks[i] == null) continue;
                    for (int j = 0; j < Tracks[i].Layout.Cnt; j++)
                    {
                        if (Tracks[i].Layout.Data[j].ProcessResult == SectorProcessResult.Bad
                            || Tracks[i].Layout.Data[j].ProcessResult == SectorProcessResult.NoHeader) cnt++;
                    }
                }
                return cnt;
            }
        }

        public int SizeTracks
        {
            get
            {
                for (int i = Tracks.Cnt - 1; i >= 0; i--)
                {
                    if (Tracks.Data[i] != null && Tracks.Data[i].FormatName != TrackFormatName.Unscanned) return i + 1;
                }
                return 0;
            }
        }

        public string FileName;
        public string FileNameOnly { get { return Path.GetFileName(FileName); } }

        public int SizeSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks[i] != null) cnt += Tracks[i].Layout.Cnt;
                }
                return cnt;
            }
        }

        public int SizeBytes
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks.Data[i] != null)
                    {
                        for (int j = 0; j < Tracks.Data[i].Layout.Cnt; j++)
                        {
                            cnt += Tracks.Data[i].Layout.Data[j].SizeBytes;
                        }
                    }
                }
                return cnt;
            }
        }

        /// <summary>
        /// Количество секторов которые не заполнены полностью нулями. Учитываются только успешно прочитанные сектора.
        /// </summary>
        public int NonZeroSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Tracks.Cnt; i++)
                {
                    if (Tracks[i] == null) continue;
                    for (int j = 0; j < Tracks[i].Layout.Cnt; j++)
                    {
                        if (Tracks[i].Layout.Data[j].ProcessResult == SectorProcessResult.Good
                            && !DiskImage.AllBytes(Tracks[i].Layout.Data[j].Data, 0, Tracks[i].Layout.Data[j].SizeBytes, 0))  // ZeroByte %%%
                            cnt++;
                    }
                }
                return cnt;
            }
        }

        public DiskImage2()
        {
            for (int i = 0; i < 172; i++)
            {
                Tracks.Add(new TrackFormat(50));
            }
        }

        public void ResetModify()
        {
            Modified = false;
        }

        public void WriteGoodSector(IntPtr memoryHandle, TrackFormat trackF, int sectorIndex)
        {
            trackF.Layout.Data[sectorIndex].WriteData(memoryHandle);
            trackF.Layout.Data[sectorIndex].ProcessResult = SectorProcessResult.Good;
            Modified = true;
            // %%% Здесь должен быть вызов Map и функции SectorsChanged(sectorNum, sectorCount);
            if (DiskImage.AllBytes(trackF.Layout.Data[sectorIndex].Data, 0, trackF.Layout.Data[sectorIndex].SizeBytes, 0))
            {
                trackF.Layout.Data[sectorIndex].MapCellValue = MapCell.Zero;
            }
            else
            {
                trackF.Layout.Data[sectorIndex].MapCellValue = MapCell.Good;    // это наверное надо перенести в Map.
            }
            trackF.MapModified = true;
        }

        public void WriteBadSector(IntPtr memoryHandle, TrackFormat trackF, int sectorIndex)
        {
            trackF.Layout.Data[sectorIndex].WriteData(memoryHandle);
            trackF.Layout.Data[sectorIndex].ProcessResult = SectorProcessResult.Bad;
            Modified = true;
            // %%% Здесь должен быть вызов Map и функции SectorsChanged(sectorNum, sectorCount);
            trackF.Layout.Data[sectorIndex].MapCellValue = MapCell.CrcError;    // это наверное надо перенести в Map.
            trackF.MapModified = true;
        }

        public void WriteNoHeader(IntPtr memoryHandle, TrackFormat trackF, int sectorIndex)
        {
            trackF.Layout.Data[sectorIndex].WriteData(memoryHandle);
            trackF.Layout.Data[sectorIndex].ProcessResult = SectorProcessResult.NoHeader;
            Modified = true;
            // %%% Здесь должен быть вызов Map и функции SectorsChanged(sectorNum, sectorCount);
            trackF.Layout.Data[sectorIndex].MapCellValue = MapCell.NoHeader;    // это наверное надо перенести в Map.
            trackF.MapModified = true;
        }

        public int LoadAutodetect(string fileName, bool modifiedTrd)
        {
            byte[] data = File.ReadAllBytes(fileName);
            string text;
            if (LoadFdi(fileName, data, out text) == 0) return 0;
            return LoadTrd(fileName, data, modifiedTrd);
        }

        public int LoadTrd(string fileName, byte[] data, bool modifiedTrd)
        {
            Clear();
            const int sectorSize = 256;
            int sizeSectors = data.Length / sectorSize;
            if (sizeSectors * sectorSize != data.Length) return 1;
            int sizeTracks = sizeSectors / 16;
            FileName = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            for (int track = 0; track < sizeTracks; track++)
            {
                TrackFormat tf = Tracks[track];
                tf.Layout.Cnt = 16;
                for (int sector = 0; sector < 16; sector++)
                {
                    tf.Layout.Data[sector].Cylinder = track / 2;
                    tf.Layout.Data[sector].Head = track & 1;
                    tf.Layout.Data[sector].SectorNumber = sector + 1;
                    tf.Layout.Data[sector].SizeCode = 1;
                    tf.Layout.Data[sector].TimeMs = 12.5;
                    tf.FormatName = TrackFormatName.TrDosSequential;
                    tf.Layout.Cnt = 16;
                    tf.SpinTime = TrackFormat.SpinTimeStandard;
                    int adr = track * 16 * 256 + sector * 256;
                    if (tf.Layout.Data[sector].Data == null || tf.Layout.Data[sector].Data.Length < 256)
                    {
                        tf.Layout.Data[sector].Data = new byte[256];
                    }
                    Array.Copy(data, adr, tf.Layout.Data[sector].Data, 0, 256);
                    tf.Layout.Data[sector].ProcessResult = SectorProcessResult.Good;
                    tf.Layout.Data[sector].MapCellValue = MapCell.Good;
                    if (modifiedTrd)
                    {
                        if (DiskImage.AllBytes(tf.Layout.Data[sector].Data, 0, 256, (byte)'B'))
                        {
                            tf.Layout.Data[sector].ProcessResult = SectorProcessResult.Bad;
                            tf.Layout.Data[sector].MapCellValue = MapCell.CrcError;
                        }
                        else if (DiskImage.AllBytes(tf.Layout.Data[sector].Data, 0, 256, (byte)'N'))
                        {
                            tf.Layout.Data[sector].ProcessResult = SectorProcessResult.Unprocessed;
                            tf.Layout.Data[sector].MapCellValue = MapCell.Unprocessed;
                        }
                    }
                    if (tf.Layout.Data[sector].MapCellValue == MapCell.Good
                        && DiskImage.AllBytes(tf.Layout.Data[sector].Data, 0, 256, 0))
                    {
                        tf.Layout.Data[sector].MapCellValue = MapCell.Zero;
                    }
                    tf.Layout.Data[sector].TimeCalculated = true;
                }
                tf.MapModified = true;
            }
            Modified = false;
            return 0;
        }

        /// <summary>
        /// 0 - Нет ошибок.
        /// 1 - Размер файла слишком мал.
        /// 5 - Размер файла слишком мал и содержит не все данные содержимого секторов.
        /// 6 - Нет сигнатуры FDI.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public unsafe int LoadFdi(string fileName, byte[] data, out string text)
        {
            Clear();
            text = null;
            if (data.Length < 14) return 1;
            if (data[0] != 'F' || data[1] != 'D' || data[2] != 'I') return 6;
            int cylCount;
            int textOffset;
            int dataOffset;
            fixed (byte* d = &data[0])
            {
                cylCount = *(ushort*)(d + 4);
                textOffset = *(ushort*)(d + 8);
                dataOffset = *(ushort*)(d + 10);
            }
            int minFileSize = 7 * cylCount * 2 + 14;
            if (data.Length < minFileSize || data.Length < textOffset + 1) return 1;

            int tt = textOffset;
            for (; tt < data.Length; tt++)
            {
                if (data[tt] == 0) break;
            }
            int textLen = tt - textOffset;
            text = textLen > 0 ? Encoding.ASCII.GetString(data, textOffset, tt - textOffset) : null;

            int index = 14;
            for (int track = 0; track < cylCount * 2; track++)
            {
                int trackOffset;
                fixed (byte* d = &data[index])
                {
                    trackOffset = *((int*)d);
                }
                int sectorsOnTrack = data[index + 6];
                TrackFormat tf = Tracks.Data[track];
                tf.Layout.EnsureCapacity(sectorsOnTrack);
                tf.Layout.Cnt = sectorsOnTrack;
                tf.SpinTime = TrackFormat.SpinTimeStandard;
                tf.MapModified = true;
                index += 7;
                double totalTimeMs = 0;
                for (int sector = 0; sector < sectorsOnTrack; sector++)
                {
                    tf.Layout.Data[sector].Cylinder = data[index];
                    tf.Layout.Data[sector].Head = data[index + 1];
                    tf.Layout.Data[sector].SectorNumber = data[index + 2];
                    tf.Layout.Data[sector].SizeCode = data[index + 3];
                    int code = data[index + 4] & 63;
                    tf.Layout.Data[sector].ProcessResult = code == 0 ? SectorProcessResult.Bad : SectorProcessResult.Good;
                    int sectorOffset;
                    fixed (byte* d = &data[index + 5])
                    {
                        sectorOffset = *((ushort*)d);
                    }
                    int sizeBytes = tf.Layout.Data[sector].SizeBytes;
                    if (data.Length < dataOffset + trackOffset + sectorOffset + sizeBytes) return 5;
                    if (tf.Layout.Data[sector].Data == null || tf.Layout.Data[sector].Data.Length < tf.Layout.Data[sector].SizeBytes)
                    {
                        tf.Layout.Data[sector].Data = new byte[tf.Layout.Data[sector].SizeBytes];
                    }
                    tf.Layout.Data[sector].MapCellValue = tf.Layout.Data[sector].ProcessResult == SectorProcessResult.Good ? MapCell.Good : MapCell.CrcError;
                    Array.Copy(data, dataOffset + trackOffset + sectorOffset, tf.Layout.Data[sector].Data, 0, sizeBytes);
                    if (tf.Layout.Data[sector].ProcessResult == SectorProcessResult.Good
                        && DiskImage.AllBytes(tf.Layout.Data[sector].Data, 0, tf.Layout.Data[sector].SizeBytes, 0))
                    {
                        tf.Layout.Data[sector].MapCellValue = MapCell.Zero;
                    }
                    index += 7;
                    tf.Layout.Data[sector].TimeCalculated = true;
                    if (sector != 0)
                    {
                        tf.Layout.Data[sector].TimeMs = (tf.Layout.Data[sector - 1].SizeBytes + TrackFormat.NormalSectorHeaderSize + 1) / TrackFormat.BytesPerMsStandard;
                        totalTimeMs += tf.Layout.Data[sector].TimeMs;
                    }
                }
                tf.FormatName = tf.GetFormatName();
                if (sectorsOnTrack > 0)
                {
                    tf.Layout.Data[0].TimeMs = Math.Max(0, TrackFormat.SpinTimeStandard - totalTimeMs);
                }
            }
            FileName = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            Modified = false;
            return 0;
        }

        public unsafe byte[] ToFdi(string text)
        {
            int trackCount = SizeTracks;
            int sizeSectors = SizeSectors;
            int cylCount = (int)Math.Ceiling(trackCount / 2.0);
            int textLength = text != null ? text.Length : 0;
            int dataSize = SizeBytes + textLength + 1 + 7 * cylCount * 2 + sizeSectors * 7 + 14;
            byte[] data = new byte[dataSize];
            data[0] = (byte)'F';
            data[1] = (byte)'D';
            data[2] = (byte)'I';
            data[4] = (byte)cylCount;
            data[6] = 2;
            //data[8] = 0;  // Смещение текста (комментария), 2 байта. Пишется ниже.
            //data[10] = 0; // Смещение данных, 2 байта. Пишется ниже.
            //data[12] = 0; // Длина дополнительной информации в заголовке, 2 байта. В этой версии - 0.
            //data[14] = 0; // Дополнительная информация ("Резерв для дальнейшей модернизации"). В текущей версии формата отсутствует.
            int index = 14;
            int trackOffset = 0;
            for (int track = 0; track < cylCount * 2; track++)
            {
                TrackFormat tf = Tracks[track];
                fixed (byte* numRef = &data[index])
                {
                    *((int*)numRef) = trackOffset;
                }
                //data[index + 4] = 0;  // 2 байта, всегда содержит 0 (резерв для модернизации формата).
                data[index + 6] = tf != null ? (byte)tf.Layout.Cnt : (byte)0;
                index += 7;
                int sectorOffset = 0;
                if (tf != null)
                {
                    for (int sector = 0; sector < tf.Layout.Cnt; sector++)
                    {
                        data[index] = (byte)(track / 2);
                        data[index + 1] = (byte)tf.Layout.Data[sector].Head;
                        data[index + 2] = (byte)tf.Layout.Data[sector].SectorNumber;
                        data[index + 3] = (byte)tf.Layout.Data[sector].SizeCode;
                        data[index + 4] = (byte)(tf.Layout.Data[sector].ProcessResult == SectorProcessResult.Good ? 1 << tf.Layout.Data[sector].SizeCode : 0);
                        fixed (byte* numRef = &data[index + 5])
                        {
                            *((ushort*)numRef) = (ushort)sectorOffset;
                        }
                        sectorOffset += tf.Layout.Data[sector].SizeBytes;
                        index += 7;
                    }
                    trackOffset += sectorOffset;
                }
            }
            int textOffset = index;
            if (textLength > 0) Encoding.ASCII.GetBytes(text, 0, text.Length, data, textOffset);
            int dataOffset = textOffset + textLength + 1;
            fixed (byte* numRef = &data[8])
            {
                *((ushort*)numRef) = (ushort)textOffset;
                *((ushort*)(numRef + 2)) = (ushort)dataOffset;
            }
            int currentOffset = 0;
            for (int track = 0; track < trackCount; track++)
            {
                TrackFormat tf = Tracks[track];
                if (tf == null) continue;
                for (int sector = 0; sector < tf.Layout.Cnt; sector++)
                {
                    if (tf.Layout.Data[sector].Data == null) continue;
                    Array.Copy(tf.Layout.Data[sector].Data, 0, data, currentOffset + dataOffset, tf.Layout.Data[sector].SizeBytes);
                    currentOffset += tf.Layout.Data[sector].SizeBytes;
                }
            }
            return data;
        }

        public void Clear()
        {
            for (int i = 0; i < Tracks.Cnt; i++)
            {
                Tracks.Data[i].SetFormat(TrackFormatName.Unscanned);
                Tracks.Data[i].MapModified = true;
            }
            Modified = false;
        }

        public void MarkSectorRange(int firstTrack, int lastTrack, SectorProcessResult processResult, MapCell mapCell)
        {
            for (int i = firstTrack; i < lastTrack; i++)
            {
                TrackFormat tf = Tracks[i];
                for (int j = 0; j < tf.Layout.Cnt; j++)
                {
                    tf.Layout.Data[j].ProcessResult = processResult;
                    tf.Layout.Data[j].MapCellValue = mapCell;
                }
                tf.MapModified = true;
            }
            Modified = true;
        }

        public void MarkTrackRangeAsUnscanned(int firstTrack, int lastTrack)
        {
            for (int i = firstTrack; i < lastTrack; i++)
            {
                Tracks[i].FormatName = TrackFormatName.Unscanned;
                Tracks[i].Layout.Cnt = 0;
                Tracks[i].MapModified = true;
            }
            Modified = true;
        }

        public void MarkSector(int track, int sectorIndex, SectorProcessResult processResult, MapCell mapCell)
        {
            Tracks[track].Layout.Data[sectorIndex].ProcessResult = processResult;
            Tracks[track].Layout.Data[sectorIndex].MapCellValue = mapCell;
            Tracks[track].MapModified = true;
            Modified = true;
        }

        /// <summary>
        /// Формирование xml-файла с данными о формате диска.
        /// </summary>
        /// <returns></returns>
        public string SaveFormatToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            int trackCount = Tracks.Cnt;
            sb.AppendLine($"<DiskFormat Name=\"{Name}\" TrackCount=\"{trackCount}\" Sectors=\"{SizeSectors}\">");
            for (int i = 0; i < trackCount; i++)
            {
                MList<SectorInfo> layout = Tracks[i].Layout;
                if (layout.Cnt != 0)
                {
                    sb.AppendLine($"  <Track Index=\"{i}\" SectorCount=\"{layout.Cnt}\" Name=\"{Tracks[i].FormatName}\">");
                    for (int j = 0; j < layout.Cnt; j++)
                    {
                        sb.AppendLine($"    <Sector Cylinder=\"{layout[j].Cylinder}\" Head=\"{layout[j].Head}\" Number=\"{layout[j].SectorNumber}\" Size=\"{layout[j].SizeCode}\" Time=\"{GP.ToString(layout[j].TimeMs, 4)}\" />");
                    }
                    sb.AppendLine("  </Track>");
                }
                else
                {
                    sb.AppendLine($"  <Track Index=\"{i}\" SectorCount=\"{layout.Cnt}\" Name=\"{Tracks[i].FormatName}\" />");
                }
            }
            sb.AppendLine("</DiskFormat>");
            return sb.ToString();
        }

        public void LoadFormatFromXml(string fileName)
        {
            int track = 0;
            MList<SectorInfo> layout = new MList<SectorInfo>(50);
            MList<SectorInfo> temp = new MList<SectorInfo>(50);
            using (XmlTextReader xml = new XmlTextReader(fileName))
            {
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.EndElement) break;
                    if (xml.NodeType != XmlNodeType.Element) continue;
                    if (xml.Name == "DiskFormat")
                    {
                        string name = xml.GetAttribute("Name");
                        if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(name)) Name = name;
                        while (xml.Read())
                        {
                            if (xml.NodeType == XmlNodeType.EndElement) break;
                            if (xml.NodeType != XmlNodeType.Element) continue;
                            switch (xml.Name)
                            {
                                case "Track":
                                    TrackFormat tf = Tracks[track];
                                    tf.Layout.EnsureCapacity(Int32.Parse(xml.GetAttribute("SectorCount")));
                                    layout.Cnt = 0;
                                    if (!xml.IsEmptyElement)
                                    {
                                        while (xml.Read())
                                        {
                                            if (xml.NodeType == XmlNodeType.EndElement) break;
                                            if (xml.NodeType != XmlNodeType.Element) continue;
                                            switch (xml.Name)
                                            {
                                                case "Sector":
                                                    SectorInfo s = new SectorInfo()
                                                    {
                                                        Cylinder = Int32.Parse(xml.GetAttribute("Cylinder")),
                                                        Head = Int32.Parse(xml.GetAttribute("Head")),
                                                        SectorNumber = Int32.Parse(xml.GetAttribute("Number")),
                                                        SizeCode = Int32.Parse(xml.GetAttribute("Size")),
                                                        TimeMs = Double.Parse(xml.GetAttribute("Time"), NumberStyles.Any, CultureInfo.InvariantCulture)
                                                    };
                                                    layout.Add(s);
                                                    break;
                                            }
                                        }
                                    }
                                    if (tf.Layout.Cnt > 0)
                                    {
                                        tf.Layout.CopyTo(temp);
                                        layout.CopyTo(tf.Layout);
                                        for (int i = 0; i < temp.Cnt; i++)
                                        {
                                            int index = tf.FindSectorIndex(temp[i].SectorNumber);
                                            if (index < 0)
                                            {
                                                Log.Info?.Out($"Сектор не найден. Трек: {track} | Сектор: {temp[i].SectorNumber}");
                                                continue;
                                            }
                                            tf.Layout.Data[index].Data = temp[i].Data;
                                            tf.Layout.Data[index].ProcessResult = temp[i].ProcessResult;
                                            tf.Layout.Data[index].MapCellValue = temp[i].MapCellValue;
                                            tf.Layout.Data[index].TimeCalculated = false;
                                        }
                                    }
                                    else
                                    {
                                        layout.CopyTo(tf.Layout);
                                        for (int i = 0; i < layout.Cnt; i++)
                                        {
                                            tf.Layout.Data[i].MapCellValue = MapCell.Unprocessed;
                                            tf.Layout.Data[i].TimeCalculated = false;
                                        }
                                    }
                                    tf.FormatName = tf.GetFormatName();
                                    tf.SpinTime = tf.GetSpinTime();
                                    tf.MapModified = true;
                                    track++;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
