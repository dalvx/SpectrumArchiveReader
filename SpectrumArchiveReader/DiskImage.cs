using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SpectrumArchiveReader
{
    public class DiskImage : ICloneable
    {
        public string Name;
        public byte[] Data;
        public SectorProcessResult[] Sectors;
        public Map Map;
        public int SectorsOnTrack;
        public int SectorSize;
        public byte ZeroByte;
        /// <summary>
        /// Формат трека: количество секторов, их номера и размер. Параметры Head, Cyl и расположение секторов значения не имеют.
        /// </summary>
        public TrackFormat StandardFormat;
        public bool Modified;

        public int ProcessedSectors
        {
            get
            {
                if (Sectors == null) return 0;
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] != SectorProcessResult.Unprocessed) cnt++;
                }
                return cnt;
            }
        }

        public int UnprocessedSectors
        {
            get
            {
                if (Sectors == null) return 0;
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Unprocessed) cnt++;
                }
                return cnt;
            }
        }

        public int GoodSectors
        {
            get
            {
                if (Sectors == null) return 0;
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Good) cnt++;
                }
                return cnt;
            }
        }

        /// <summary>
        /// Количество секторов Bad и NoHeader.
        /// </summary>
        public int NotGoodSectors
        {
            get
            {
                if (Sectors == null) return 0;
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Bad || Sectors[i] == SectorProcessResult.NoHeader) cnt++;
                }
                return cnt;
            }
        }

        public int BadSectors
        {
            get
            {
                if (Sectors == null) return 0;
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Bad) cnt++;
                }
                return cnt;
            }
        }

        /// <summary>
        /// Количество секторов при сохранении в файл, после удаления всех необработанных секторов в конце диска.
        /// </summary>
        public int FileSectorsSize
        {
            get
            {
                if (Sectors == null) return 0;
                int i;
                for (i = Sectors.Length - 1; i >= 0; i--)
                {
                    if (Sectors[i] != SectorProcessResult.Unprocessed) break;
                }
                return i + 1;
            }
        }

        public string FileName;
        public string FileNameOnly { get { return Path.GetFileName(FileName); } }

        public int SizeSectors { get { return Sectors != null ? Sectors.Length : 0; } }

        public int SizeTracks { get { return Sectors != null ? (int)Math.Ceiling((double)Sectors.Length / SectorsOnTrack) : 0; } }

        /// <summary>
        /// Количество секторов которые не заполнены полностью нулями. Учитываются только успешно прочитанные сектора.
        /// </summary>
        public int NonZeroSectors
        {
            get
            {
                if (Sectors == null) return 0;
                int cntr = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Good && !AllBytes(Data, i * SectorSize, SectorSize, ZeroByte)) cntr++;
                }
                return cntr;
            }
        }

        public DiskImage(int sectorSize, int sectorsOnTrack, int sizeSectors, Map map)
        {
            SectorSize = sectorSize;
            SectorsOnTrack = sectorsOnTrack;
            if (sizeSectors > 0)
            {
                Data = new byte[sizeSectors * sectorSize];
                Sectors = new SectorProcessResult[sizeSectors];
                Map = map;
                SectorsChanged(0, sizeSectors);
                Modified = false;
            }
        }

        public DiskImage()
        {

        }

        public int GetNotGoodBounds(int firstSectorNum, int lastSectorNum, DiskSide side, ref int begin, ref int end)
        {
            int cntr = 0;
            int b = -1;
            for (int i = firstSectorNum; i < lastSectorNum; i++)
            {
                if (Sectors[i] == SectorProcessResult.Good) continue;
                int track = i / SectorsOnTrack;
                if ((track & 1) == 0 && (side == DiskSide.Side1)) continue;
                if ((track & 1) == 1 && (side == DiskSide.Side0)) continue;
                if (b < 0) b = i;
                end = i + 1;
                cntr++;
            }
            begin = b;
            return cntr;
        }

        public virtual void SectorsChanged(int index, int length = 1)
        {
            Map?.ModifySectors(index, length, Data, Sectors);
        }

        public void SetSize(int sizeSectors)
        {
            if (Sectors.Length == sizeSectors) return;
            int newLen = sizeSectors * SectorSize;
            int oldSize = Sectors.Length;
            byte[] newData = new byte[newLen];
            Array.Copy(Data, 0, newData, 0, Math.Min(Data.Length, newLen));
            Data = newData;
            SectorProcessResult[] newSectors = new SectorProcessResult[sizeSectors];
            Array.Copy(Sectors, 0, newSectors, 0, Math.Min(Sectors.Length, sizeSectors));
            Sectors = newSectors;
            SectorsChanged(Math.Min(oldSize, sizeSectors), Math.Abs(oldSize - sizeSectors));
        }

        /// <summary>
        /// 0 - Нет ошибок.
        /// 1 - Размер файла слишком мал.
        /// 2 - Число секторов на одном из треков не соответствует установленному числу секторов образа.
        /// 3 - Сектор файла имеет номер отсутствующий в списке секторов установленных для образа.
        /// 4 - Размер одного из секторов файла не соответствует размеру секторов образа.
        /// 5 - Размер файла слишком мал и содержит не все данные содержимого секторов.
        /// 6 - Нет сигнатуры FDI.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public unsafe int LoadFdi(string fileName, byte[] data, out string text, Map map = null)
        {
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
            int minFileSize = (7 + SectorsOnTrack * 7) * cylCount * 2 + 14;
            if (data.Length < minFileSize || data.Length < textOffset + 1) return 1;

            int tt = textOffset;
            for (; tt < data.Length; tt++)
            {
                if (data[tt] == 0) break;
            }
            int textLen = tt - textOffset;
            text = textLen > 0 ? Encoding.ASCII.GetString(data, textOffset, tt - textOffset) : null;

            int sizeSectors = cylCount * SectorsOnTrack * 2;
            Data = new byte[sizeSectors * SectorSize];
            Sectors = new SectorProcessResult[sizeSectors];
            int index = 14;
            for (int track = 0; track < cylCount * 2; track++)
            {
                int trackOffset;
                fixed (byte* d = &data[index])
                {
                    trackOffset = *((int*)d);
                }
                int sectorsOnTrack = data[index + 6];
                if (sectorsOnTrack > SectorsOnTrack) return 2;
                for (int sectorNum = track * SectorsOnTrack; sectorNum < (track + 1) * SectorsOnTrack; sectorNum++)
                {
                    Sectors[sectorNum] = SectorProcessResult.NoHeader;
                }
                index += 7;
                for (int sector = 0; sector < sectorsOnTrack; sector++)
                {
                    int diskSectorNum = data[index + 2];
                    int sectorSizeCode = data[index + 3];
                    int sectorIndex = StandardFormat.FindSectorIndex(diskSectorNum);
                    if (sectorIndex < 0) return 3;
                    if (StandardFormat.Layout.Data[sectorIndex].SizeCode != sectorSizeCode) return 4;
                    int imageSectorNum = track * SectorsOnTrack + sectorIndex;
                    int code = data[index + 4] & 63;
                    Sectors[imageSectorNum] = code == 0 ? SectorProcessResult.Bad : SectorProcessResult.Good;
                    int sectorOffset;
                    fixed (byte* d = &data[index + 5])
                    {
                        sectorOffset = *((ushort*)d);
                    }
                    if (data.Length < dataOffset + trackOffset + sectorOffset + SectorSize) return 5;
                    Array.Copy(data, dataOffset + trackOffset + sectorOffset, Data, imageSectorNum * SectorSize, SectorSize);
                    index += 7;
                }
            }
            FileName = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            Map = map;
            SectorsChanged(0, sizeSectors);
            Modified = false;
            return 0;
        }

        public unsafe byte[] ToFdi(string text, int minSizeSectors)
        {
            int trackCount = (int)Math.Ceiling((double)Math.Max(FileSectorsSize, minSizeSectors) / SectorsOnTrack);
            int cylCount = (int)Math.Ceiling(trackCount / 2.0);
            int textLength = text != null ? text.Length : 0;
            int totalExistingSectors = GoodSectors + BadSectors;
            int dataSize = totalExistingSectors * SectorSize + textLength + 1 + 7 * cylCount * 2 + totalExistingSectors * 7 + 14;
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
                int sectorCount = 0;
                for (int sector = track * SectorsOnTrack; sector < (track + 1) * SectorsOnTrack; sector++)
                {
                    if (sector < Sectors.Length && (Sectors[sector] == SectorProcessResult.Good || Sectors[sector] == SectorProcessResult.Bad)) sectorCount++;
                }

                fixed (byte* numRef = &data[index])
                {
                    // Смещение трека - начало области данных этого трека относительно "Смещения данных" 
                    *((int*)numRef) = trackOffset;
                }
                //data[index + 4] = 0;  // 2 байта, всегда содержит 0 (резерв для модернизации формата).
                data[index + 6] = (byte)sectorCount;
                index += 7;
                int sectorOffset = 0;
                for (int sector = 0; sector < SectorsOnTrack; sector++)
                {
                    int sectorNum = track * SectorsOnTrack + sector;
                    if (sectorNum >= Sectors.Length || (Sectors[sectorNum] != SectorProcessResult.Good && Sectors[sectorNum] != SectorProcessResult.Bad)) continue;
                    data[index] = (byte)(track / 2);
                    data[index + 1] = (byte)(track & 1);
                    data[index + 2] = (byte)StandardFormat.Layout.Data[sector].SectorNumber;
                    data[index + 3] = (byte)StandardFormat.Layout.Data[sector].SizeCode;
                    data[index + 4] = (byte)(sectorNum >= Sectors.Length || Sectors[sectorNum] == SectorProcessResult.Good ? 1 << StandardFormat.Layout.Data[sector].SizeCode : 0);
                    fixed (byte* numRef = &data[index + 5])
                    {
                        *((ushort*)numRef) = (ushort)sectorOffset;
                    }
                    sectorOffset += StandardFormat.Layout.Data[sector].SizeBytes;
                    index += 7;
                }
                trackOffset += sectorOffset;
            }
            int textOffset = index;
            if (textLength > 0) Encoding.ASCII.GetBytes(text, 0, text.Length, data, textOffset);
            int dataOffset = textOffset + textLength + 1;
            fixed (byte* numRef = &data[8])
            {
                *((ushort*)numRef) = (ushort)textOffset;
                *((ushort*)(numRef + 2)) = (ushort)dataOffset;
            }
            int currentSectorNum = 0;
            int sizeSectors = FileSectorsSize;
            for (int track = 0; track < trackCount; track++)
            {
                for (int sector = 0; sector < SectorsOnTrack; sector++)
                {
                    int sectorNum = track * SectorsOnTrack + sector;
                    if (sectorNum >= Sectors.Length || (Sectors[sectorNum] != SectorProcessResult.Good && Sectors[sectorNum] != SectorProcessResult.Bad)) continue;
                    if (sectorNum < sizeSectors) Array.Copy(Data, sectorNum * SectorSize, data, currentSectorNum * SectorSize + dataOffset, SectorSize);
                    currentSectorNum++;
                }
            }
            return data;
        }

        /// <summary>
        /// Загрузка формата аналогичного TRD.
        /// 0 - Нет ошибок.
        /// 1 - Размер файла не кратен размеру сектора.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sizeSectors"></param>
        /// <param name="map"></param>
        public int LoadTrd(string fileName, byte[] data, bool modifiedTrd, Map map = null)
        {
            int sizeSectors = data.Length / SectorSize;
            if (sizeSectors * SectorSize != data.Length) return 1;
            Data = new byte[sizeSectors * SectorSize];
            Sectors = new SectorProcessResult[sizeSectors];
            FileName = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            Array.Copy(data, 0, Data, 0, Math.Min(data.Length, Data.Length));
            int last = Math.Min(sizeSectors, data.Length / SectorSize);
            Map = map;
            if (modifiedTrd)
            {
                for (int i = 0; i < last; i++)
                {
                    int adr = i * SectorSize;
                    if (AllBytes(data, adr, SectorSize, (byte)'B'))
                    {
                        Sectors[i] = SectorProcessResult.Bad;
                    }
                    else if (AllBytes(data, adr, SectorSize, (byte)'N'))
                    {
                        Sectors[i] = SectorProcessResult.Unprocessed;
                    }
                    else
                    {
                        Sectors[i] = SectorProcessResult.Good;
                    }
                }
            }
            else
            {
                for (int i = 0; i < last; i++)
                {
                    Sectors[i] = SectorProcessResult.Good;
                }
            }
            for (int i = last; i < sizeSectors; i++)
            {
                Sectors[i] = SectorProcessResult.Unprocessed;
            }
            SectorsChanged(0, sizeSectors);
            Modified = false;
            return 0;
        }

        public int LoadAutodetect(string fileName, Map map = null)
        {
            byte[] data = File.ReadAllBytes(fileName);
            string text;
            if (LoadFdi(fileName, data, out text, map) == 0) return 0;
            return LoadTrd(fileName, data, true, map);
        }

        public void Merge(DiskImage image, out int addedReadSectors)
        {
            int size = Math.Max(image.Data.Length, Data.Length);
            int sizeSectors = size / SectorSize;
            byte[] newData = new byte[size];
            SectorProcessResult[] sectors = new SectorProcessResult[sizeSectors];
            addedReadSectors = 0;
            for (int i = 0; i < sizeSectors; i++)
            {
                if (i < Sectors.Length && Sectors[i] == SectorProcessResult.Good)
                {
                    Array.Copy(Data, i * SectorSize, newData, i * SectorSize, SectorSize);
                    sectors[i] = SectorProcessResult.Good;
                }
                else if (i < image.Sectors.Length && image.Sectors[i] == SectorProcessResult.Good)
                {
                    Array.Copy(image.Data, i * SectorSize, newData, i * SectorSize, SectorSize);
                    sectors[i] = SectorProcessResult.Good;
                    addedReadSectors++;
                }
                else if (i < Sectors.Length && Sectors[i] == SectorProcessResult.Bad)
                {
                    sectors[i] = SectorProcessResult.Bad;
                }
                else if (i < image.Sectors.Length && image.Sectors[i] == SectorProcessResult.Bad)
                {
                    sectors[i] = SectorProcessResult.Bad;
                }
                else
                {
                    sectors[i] = SectorProcessResult.Unprocessed;
                }
            }
            Data = newData;
            Sectors = sectors;
            SectorsChanged(0, Sectors.Length);
        }

        public void WriteGoodSectors(int sectorNum, IntPtr memoryHandle, int sectorCount)
        {
            Marshal.Copy(memoryHandle, Data, sectorNum * SectorSize, Math.Min(Sectors.Length - sectorNum, sectorCount) * SectorSize);
            for (int i = sectorNum, last = Math.Min(sectorNum + sectorCount, Sectors.Length); i < last; i++)
            {
                Sectors[i] = SectorProcessResult.Good;
            }
            SectorsChanged(sectorNum, sectorCount);
            Modified = true;
        }

        public void WriteBadSectors(int sectorNum, IntPtr memoryHandle, int sectorCount, bool noHeader)
        {
            Marshal.Copy(memoryHandle, Data, sectorNum * SectorSize, Math.Min(Sectors.Length - sectorNum, sectorCount) * SectorSize);
            for (int i = sectorNum, ilast = Math.Min(sectorNum + sectorCount, Sectors.Length); i < ilast; i++)
            {
                Sectors[i] = noHeader ? SectorProcessResult.NoHeader : SectorProcessResult.Bad;
            }
            SectorsChanged(sectorNum, sectorCount);
            Modified = true;
        }

        public void WriteBadSectors(int sectorNum, int sectorCount, bool noHeader)
        {
            for (int i = sectorNum, ilast = Math.Min(sectorNum + sectorCount, Sectors.Length); i < ilast; i++)
            {
                Sectors[i] = noHeader ? SectorProcessResult.NoHeader : SectorProcessResult.Bad;
            }
            SectorsChanged(sectorNum, sectorCount);
            Modified = true;
        }

        public void SetSectorsProcessResult(SectorProcessResult processResult, int index, int length = 1)
        {
            for (int i = index, last = Math.Min(index + length, Sectors.Length); i < last; i++)
            {
                Sectors[i] = processResult;
            }
            SectorsChanged(index, length);
            Modified = true;
        }

        public void ResetModify()
        {
            Modified = false;
        }

        public static bool AllBytes(byte[] data, int index, int length, byte value)
        {
            for (int i = index; i < index + length; i++)
            {
                if (data[i] != value) return false;
            }
            return true;
        }

        protected static void Fill(byte[] data, int index, int length, byte value)
        {
            for (int i = index; i < index + length; i++)
            {
                data[i] = value;
            }
        }

        protected string ReplaceZeroInString(string s)
        {
            StringBuilder sb = new StringBuilder(s);
            for (int k = 0; k < s.Length; k++)
            {
                if (sb[k] == 0)
                {
                    sb[k] = (char)0x24ff;
                }
                else if (sb[k] == 1)
                {
                    sb[k] = (char)0x2776;
                }
                else if (sb[k] < 32 || sb[k] >= 127)
                {
                    sb[k] = '?';
                }
            }
            return sb.ToString();
        }

        public string BuildDuplicateSectorsMap()
        {
            StringBuilder sb = new StringBuilder(Sectors.Length);
            sb.Length = Sectors.Length;
            for (int i = 0; i < Sectors.Length; i++)
            {
                sb[i] = 'o';
            }
            for (int i = 0; i < Sectors.Length - 1; i++)
            {
                if (AllBytes(Data, i * SectorSize, SectorSize, 0)
                    || AllBytes(Data, i * SectorSize, SectorSize, (byte)'B')
                    || AllBytes(Data, i * SectorSize, SectorSize, (byte)'N'))
                    continue;
                for (int j = i + 1; j < Sectors.Length; j++)
                {
                    if (AllBytes(Data, j * SectorSize, SectorSize, 0)
                        || AllBytes(Data, j * SectorSize, SectorSize, (byte)'B')
                        || AllBytes(Data, j * SectorSize, SectorSize, (byte)'N'))
                        continue;
                    for (int d = i * SectorSize, f = j * SectorSize, last = f + SectorSize; f < last; d++, f++)
                    {
                        if (Data[d] != Data[f]) goto noMatch;
                    }
                    sb[i] = 'x';
                    sb[j] = 'x';
                    return sb.ToString();
                    noMatch:;
                }
            }
            return sb.ToString();
        }

        public string BuildAdjacentSectorsDuplicateSectorsMap(int adjacentSectors)
        {
            StringBuilder sb = new StringBuilder(Sectors.Length);
            sb.Length = Sectors.Length;
            for (int i = 0; i < Sectors.Length; i++)
            {
                sb[i] = 'o';
            }
            byte[] workArray = new byte[SectorSize];
            for (int i = 0; i < Sectors.Length - 1; i++)
            {
                if (Sectors[i] != SectorProcessResult.Good) continue;
                if (AllBytes(Data, i * SectorSize, SectorSize, Data[i * SectorSize])) continue;
                ContentType content = GetContentType(workArray, Data, i * SectorSize, SectorSize);
                if (content == ContentType.Graphics || content == ContentType.Zero) continue;
                int sectorFrom = Math.Max(i - adjacentSectors, 0);
                int track = i / SectorsOnTrack;
                //if (sectorFrom / SectorsOnTrack < track) sectorFrom += SectorsOnTrack;
                int sectorTo = Math.Min(i + adjacentSectors, Sectors.Length - 1);
                //if (sectorTo / SectorsOnTrack > track) sectorTo -= SectorsOnTrack;
                for (int j = sectorFrom; j <= sectorTo; j++)
                {
                    if (j == i) continue;
                    for (int d = i * SectorSize, f = j * SectorSize, last = f + SectorSize; f < last; d++, f++)
                    {
                        if (Data[d] != Data[f]) goto noMatch;
                    }
                    sb[i] = 'x';
                    sb[j] = 'x';
                    //sreturn sb.ToString();
                    noMatch:;
                }
            }
            return sb.ToString();
        }

        public string BuildSectorContentMap()
        {
            StringBuilder sb = new StringBuilder(Sectors.Length);
            byte[] array = new byte[SectorSize];
            for (int i = 0; i < Sectors.Length; i++)
            {
                if (Sectors[i] != SectorProcessResult.Good)
                {
                    sb.Append('b');
                    continue;
                }
                ContentType content = GetContentType(array, Data, i * SectorSize, SectorSize);
                switch (content)
                {
                    case ContentType.Unrecognized:
                        sb.Append('o');
                        break;

                    case ContentType.Zero:
                        sb.Append('z');
                        break;

                    case ContentType.Code:
                        sb.Append('c');
                        break;

                    case ContentType.Text:
                        sb.Append('t');
                        break;

                    case ContentType.Tasm4:
                        sb.Append('a');
                        break;

                    case ContentType.Gens:
                        sb.Append('e');
                        break;

                    case ContentType.Graphics:
                        sb.Append('f');
                        break;
                }
            }
            return sb.ToString();
        }

        public ContentType GetContentType(byte[] workArray, byte[] data, int index, int length)
        {
            if (AllBytes(data, index, length, ZeroByte)) return ContentType.Zero;
            int cd = 0;
            int oa = 0;
            int b3LD = 0;
            int o8 = 0;
            int asmCommand = 0;
            int space = 0;
            int comma = 0;
            Array.Clear(workArray, 0, SectorSize);
            int bigAscii = 0;
            int ascii = 0;
            for (int c = index, last = c + length; c < last; c++)
            {
                workArray[data[c]]++;
                if (data[c] >= 32 && data[c] < 92) bigAscii++;
                if (data[c] >= 32 && data[c] < 123) ascii++;
                if (data[c] == 0x0a)
                {
                    oa++;
                }
                if (data[c] == 0xb3)
                {
                    b3LD++;
                }
                else if (data[c] == 0x08)
                {
                    o8++;
                }
                else if (data[c] == 0xcd)
                {
                    cd++;
                }
                else if (data[c] == 32)
                {
                    space++;
                }
                else if (data[c] == ',')
                {
                    comma++;
                }
                if (c > index)
                {
                    if (c < last - 4)
                    {
                        if ((data[c] <= 32 || data[c] > 123) && (data[c + 3] <= 32 || data[c + 3] > 123))
                        {
                            if (data[c + 1] == 'L' && data[c + 2] == 'D')
                            {
                                asmCommand++;
                            }
                        }
                    }
                    if (c < last - 5)
                    {
                        if ((data[c] <= 32 || data[c] > 123) && (data[c + 4] <= 32 || data[c + 4] > 123))
                        {
                            if ((data[c + 1] == 'R' && data[c + 2] == 'E' && data[c + 3] == 'T')
                                || (data[c + 1] == 'D' && data[c + 2] == 'E' && data[c + 3] == 'C')
                                || (data[c + 1] == 'I' && data[c + 2] == 'N' && data[c + 3] == 'C')
                                || (data[c + 1] == 'P' && data[c + 2] == 'O' && data[c + 3] == 'P')
                                || (data[c + 1] == 'A' && data[c + 2] == 'D' && data[c + 3] == 'D'))
                            {
                                asmCommand++;
                            }
                        }
                    }
                    if (c < last - 6)
                    {
                        if ((data[c] <= 32 || data[c] > 123) && (data[c + 5] <= 32 || data[c + 5] > 123))
                        {
                            if ((data[c + 1] == 'C' && data[c + 2] == 'A' && data[c + 3] == 'L' && data[c + 4] == 'L')
                                || (data[c + 1] == 'P' && data[c + 2] == 'U' && data[c + 3] == 'S' && data[c + 4] == 'H'))
                            {
                                asmCommand++;
                            }
                        }
                    }
                }
            }
            int byteCnt = 0;
            for (int c = 0; c < SectorSize; c++)
            {
                if (workArray[c] != 0) byteCnt++;
            }

            // TASM4.0 Detection
            //for (int c = index, last = c + length / 2; c < last; c++)
            //{
            //    int cnt = 0;
            //    for (int t = c; t < last; t++)
            //    {
            //        int len = data[t];
            //        t += len + 1;
            //        if (t >= index + length) break;
            //        if (data[t] != len) break;
            //        if (len > 0) cnt++;
            //    }
            //    if (cnt > 3) return ContentType.Tasm4;
            //}

            if (oa > 6 && (oa + bigAscii + b3LD + o8) > 100 / 256.0 * length)
            {
                return ContentType.Tasm4;
            }
            else if (asmCommand > 2)
            {
                return ContentType.Gens;
            }
            else if (ascii > 200 / 256.0 * length)
            {
                return ContentType.Text;
            }
            else if (byteCnt < 50 / 256.0 * length)
            {
                return ContentType.Graphics;
            }
            else if (cd > 4)
            {
                return ContentType.Code;
            }
            else
            {
                return ContentType.Unrecognized;
            }
        }

        public virtual object Clone()
        {
            DiskImage image = new DiskImage();
            image.Data = new byte[Data.Length];
            Data.CopyTo(image.Data, 0);
            image.Sectors = new SectorProcessResult[Sectors.Length];
            Sectors.CopyTo(image.Sectors, 0);
            image.Name = Name;
            image.FileName = FileName;
            return image;
        }
    }

    public enum SectorProcessResult
    {
        Unprocessed,
        Good,
        Bad,
        NoHeader
    }

    public enum DiskType
    {
        Unidentified,
        DS80,
        DS40,
        SS80,
        SS40
    }

    public enum ContentType
    {
        Unrecognized,
        Zero,
        Code,
        Text,
        Tasm4,
        Gens,
        Graphics
    }

    public enum FileType
    {
        Fdi,
        Trd,
        ModifiedTrd,
        Kdi
    }
}
