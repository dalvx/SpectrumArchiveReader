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
        protected DiskImage original;

        public bool Modified
        {
            get
            {
                if (Data.Length != original.Data.Length) return true;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] != original.Sectors[i]) return true;
                }
                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i] != original.Data[i]) return true;
                }
                return false;
            }
        }

        public int ProcessedSectors
        {
            get
            {
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
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Good) cnt++;
                }
                return cnt;
            }
        }

        public int BadSectors
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < Sectors.Length; i++)
                {
                    if (Sectors[i] == SectorProcessResult.Bad || Sectors[i] == SectorProcessResult.NoHeader) cnt++;
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

        public int SizeSectors { get { return Sectors.Length; } }

        public int SizeTracks { get { return (int)Math.Ceiling((double)Sectors.Length / SectorsOnTrack); } }

        /// <summary>
        /// Количество секторов которые не заполнены полностью нулями. Учитываются только успешно прочитанные сектора.
        /// </summary>
        public int NonZeroSectors
        {
            get
            {
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
                original = (DiskImage)Clone();
            }
        }

        public DiskImage()
        {

        }

        public int GetNotGoodBounds(int sectorNumFrom, int sectorNumTo, DiskSide side, ref int begin, ref int end)
        {
            int cntr = 0;
            int b = -1;
            for (int i = sectorNumFrom; i < sectorNumTo; i++)
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

        public void Load(string fileName, int sizeSectors = 0, Map map = null)
        {
            byte[] data = File.ReadAllBytes(fileName);
            if (sizeSectors == 0)
            {
                sizeSectors = data.Length / SectorSize;
                if (sizeSectors * SectorSize != data.Length)
                {
                    Log.Warn?.Out($"Размер файла не кратен размеру сектора. Размер: {data.Length} | FileName: {fileName}");
                }
            }
            Data = new byte[sizeSectors * SectorSize];
            Sectors = new SectorProcessResult[sizeSectors];
            FileName = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            Array.Copy(data, 0, Data, 0, Math.Min(data.Length, Data.Length));
            int last = Math.Min(sizeSectors, data.Length / SectorSize);
            Map = map;
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
            for (int i = last; i < sizeSectors; i++)
            {
                Sectors[i] = SectorProcessResult.Unprocessed;
            }
            SectorsChanged(0, sizeSectors);
            original = (DiskImage)Clone();
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
        }

        public void WriteBadSectors(int sectorNum, int sectorCount, bool noHeader)
        {
            for (int i = sectorNum, ilast = Math.Min(sectorNum + sectorCount, Sectors.Length); i < ilast; i++)
            {
                Sectors[i] = noHeader ? SectorProcessResult.NoHeader : SectorProcessResult.Bad;
            }
            SectorsChanged(sectorNum, sectorCount);
        }

        public void SetSectorsProcessResult(SectorProcessResult processResult, int index, int length = 1)
        {
            for (int i = index, last = Math.Min(index + length, Sectors.Length); i < last; i++)
            {
                Sectors[i] = processResult;
            }
            SectorsChanged(index, length);
        }

        public void ResetModify()
        {
            original = (DiskImage)Clone();
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
}
