using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpectrumArchiveReader
{
    public class TrDosImage : DiskImage
    {
        private bool catalogueParsed;
        public MList<FileData> Files;
        public string Title;
        /// <summary>
        /// Свободное место на диске из 8-го сектора.
        /// </summary>
        public int Free8Sector;
        /// <summary>
        /// Количество файлов записанное в 8-м секторе. Равно количеству файлов + количеству удаленных файлов.
        /// </summary>
        public int FileCount8Sector;
        /// <summary>
        /// Тип диска из 8 сектора.
        /// </summary>
        public DiskType DiskType;
        /// <summary>
        /// Количество удаленных файлов записанное в 8-м секторе.
        /// </summary>
        public int DeletedFiles8Sector;

        /// <summary>
        /// Если каталог прочитан, то возвращает true.
        /// </summary>
        public bool CatIsRead
        {
            get
            {
                if (Sectors.Length < 9) return false;
                for (int i = 0; i < 9; i++)
                {
                    if (Sectors[i] != SectorProcessResult.Good) return false;
                }
                return true;
            }
        }

        public int DamagedFiles
        {
            get
            {
                int damagedFilesCount = 0;
                for (int i = 0, adr = 0; i < 128; i++, adr += 16)
                {
                    if (Sectors == null || adr / SectorSize >= Sectors.Length) break;
                    if (Sectors[adr / SectorSize] != SectorProcessResult.Good) continue;
                    if (Data[adr] == 0) break;
                    int size = Data[adr + 13];
                    int sector = Data[adr + 14];
                    int track = Data[adr + 15];
                    int sectorNum = track * SectorsOnTrack + sector;
                    for (int j = sectorNum, last = sectorNum + size; j < last; j++)
                    {
                        if (j >= Sectors.Length || Sectors[j] != SectorProcessResult.Good)
                        {
                            damagedFilesCount++;
                            break;
                        }
                    }
                }
                return damagedFilesCount;
            }
        }

        public int FileCountUntil0
        {
            get
            {
                int cnt = 0;
                for (int i = 0, adr = 0; i < 128; i++, adr += 16)
                {
                    int csector = adr / SectorSize;
                    if (Sectors == null || csector >= Sectors.Length) break;
                    if (Sectors[csector] != SectorProcessResult.Good) continue;
                    if (Data[adr] == 0) break;
                    cnt++;
                }
                return cnt;
            }
        }

        public TrDosImage(int sizeSectors, Map map) : base(256, 16, sizeSectors, map)
        {
            StandardFormat = TrackFormat.TrDos;
            map?.BuildMap(Data, Sectors);
        }

        public TrDosImage() : base(256, 16, 0, null)
        {
            StandardFormat = TrackFormat.TrDos;
        }

        public override void SectorsChanged(int index, int length = 1)
        {
            base.SectorsChanged(index, length);
            if (index < 9) catalogueParsed = false;
        }

        public void ParseCatalogue(int sectorNumber = 0, bool strictTrdosFormat = true)
        {
            if (Files == null)
            {
                Files = new MList<FileData>();
            }
            else
            {
                Files.Clear();
            }
            for (int i = 0, adr = sectorNumber * SectorSize; i < 128; i++, adr += 16)
            {
                if (Sectors == null || adr / SectorSize >= Sectors.Length) break;
                if (adr + 16 > Data.Length) break;
                if (Sectors[adr / SectorSize] != SectorProcessResult.Good) continue;
                if (strictTrdosFormat && Data[adr] == 0) break;
                if (AllBytes(Data, adr, 8, 0)) continue;
                FileData fileData = new FileData();
                fileData.FileName = ReplaceZeroInString(Encoding.ASCII.GetString(Data, adr, 9));
                fileData.Extension = fileData.FileName[8];
                fileData.FileName = fileData.FileName.Substring(0, 8);
                fileData.Start = Data[adr + 9] + Data[adr + 10] * SectorSize;
                fileData.Length = Data[adr + 11] + Data[adr + 12] * SectorSize;
                fileData.Size = Data[adr + 13];
                fileData.Sector = Data[adr + 14];
                fileData.Track = Data[adr + 15];
                int diskAddress = fileData.Track * SectorsOnTrack + fileData.Sector;
                int good = 0;
                int bad = 0;
                int unprocessed = 0;
                for (int j = diskAddress, last = Math.Min(Sectors.Length, diskAddress + fileData.Size); j < last; j++)
                {
                    switch (Sectors[j])
                    {
                        case SectorProcessResult.Unprocessed:
                            unprocessed++;
                            break;

                        case SectorProcessResult.Good:
                            good++;
                            break;

                        case SectorProcessResult.Bad:
                        case SectorProcessResult.NoHeader:
                            bad++;
                            break;
                    }
                }
                fileData.GoodSectors = good;
                fileData.BadSectors = bad;
                fileData.UnprocessedSectors = unprocessed;
                Files.Add(fileData);
            }
            if (Sectors.Length > sectorNumber + 8 && Sectors[sectorNumber + 8] == SectorProcessResult.Good)
            {
                Title = ReplaceZeroInString(Encoding.ASCII.GetString(Data, (sectorNumber + 8) * SectorSize + 245, 8));
                Free8Sector = Data[(sectorNumber + 8) * SectorSize + 229] + Data[(sectorNumber + 8) * SectorSize + 230] * SectorSize;
                DeletedFiles8Sector = Data[(sectorNumber + 8) * SectorSize + 244];
                FileCount8Sector = Data[(sectorNumber + 8) * SectorSize + 228];
                switch (Data[(sectorNumber + 8) * SectorSize + 227])
                {
                    case 0x16:
                        DiskType = DiskType.DS80;
                        break;

                    case 0x17:
                        DiskType = DiskType.DS40;
                        break;

                    case 0x18:
                        DiskType = DiskType.SS80;
                        break;

                    case 0x19:
                        DiskType = DiskType.SS40;
                        break;

                    default:
                        DiskType = DiskType.Unidentified;
                        break;
                }
            }
            else
            {
                Title = null;
                Free8Sector = 0;
                DeletedFiles8Sector = 0;
                FileCount8Sector = 0;
                DiskType = DiskType.Unidentified;
            }
            catalogueParsed = true;
        }

        public string ToHtmlTableAsFileList(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + "<table>");
            sb.AppendLine(indent + "    " + $"<tr><th>Filename</th><td>{Path.GetFileName(FileName)}</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Title</th><td>{Title}</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Files</th><td>{(FileCount8Sector - DeletedFiles8Sector)} ({Files.Cnt})</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Deleted files</th><td>{DeletedFiles8Sector}</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Sectors read</th><td>{ProcessedSectors}</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Good sectors</th><td>{GoodSectors}</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Bad sectors</th><td>{BadSectors}</td></tr>");
            sb.AppendLine(indent + "    " + $"<tr><th>Non-Zero sectors</th><td>{NonZeroSectors}</td></tr>");
            sb.AppendLine(indent + "</table>");
            sb.AppendLine(indent + "<table>");
            sb.AppendLine(indent + "    " + "<tr><th>Filename</th><th>Ext</th><th>Size</th><th>Good</th><th>Track</th><th>Sector</th></tr>");
            for (int i = 0; i < Files.Cnt; i++)
            {
                string red = Files[i].GoodSectors == Files[i].Size ? ">" : " class = \"r\">";
                sb.Append(indent + "    " + $"<tr><td{red}{Files[i].FileName}</td><td{red}{Files[i].Extension}</td><td>{Files[i].Size}</td><td{red}{Files[i].GoodSectors}</td><td>{Files[i].Track}</td><td>{Files[i].Sector}</td></tr>");
            }
            sb.AppendLine(indent + "</table>");
            return sb.ToString();
        }

        public string GetSectorsAsString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Sectors.Length; i++)
            {
                switch (Sectors[i])
                {
                    case SectorProcessResult.Unprocessed:
                        sb.Append("u");
                        break;

                    case SectorProcessResult.Good:
                        if (AllBytes(Data, i * SectorSize, SectorSize, 0))
                        {
                            sb.Append("z");
                        }
                        else
                        {
                            sb.Append("g");
                        }
                        break;

                    default:
                        sb.Append("b");
                        break;
                }
            }
            for (int i = 0; i < Files.Cnt; i++)
            {
                int sectorNum = Files[i].Track * SectorsOnTrack + Files[i].Sector;
                for (int j = sectorNum, last = Math.Min(sectorNum + Files[i].Size, Sectors.Length); j < last; j++)
                {
                    switch (sb[j])
                    {
                        case 'u':
                            sb[j] = 'U';
                            break;

                        case 'g':
                            sb[j] = 'G';
                            break;

                        case 'z':
                            sb[j] = 'Z';
                            break;

                        case 'b':
                            sb[j] = 'B';
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        public byte[] ToTrd(int minSizeSectors, bool modified)
        {
            int size = Math.Max(FileSectorsSize, minSizeSectors);
            byte[] r = new byte[size * SectorSize];
            Array.Copy(Data, 0, r, 0, Math.Min(r.Length, Data.Length));
            if (!modified) return r;
            for (int i = 0; i < size; i++)
            {
                if (i < Sectors.Length)
                {
                    switch (Sectors[i])
                    {
                        case SectorProcessResult.Unprocessed:
                            Fill(r, i * SectorSize, SectorSize, (byte)'N');
                            break;

                        case SectorProcessResult.Bad:
                        case SectorProcessResult.NoHeader:
                            Fill(r, i * SectorSize, SectorSize, (byte)'B');
                            break;
                    }
                }
                else
                {
                    Fill(r, i * SectorSize, SectorSize, (byte)'N');
                }
            }
            return r;
        }

        public FileData GetFileByDiskAddress(int track, int sector)
        {
            int diskAddress = track * SectorsOnTrack + sector;
            if (!catalogueParsed) ParseCatalogue();
            for (int i = 0; i < Files.Cnt; i++)
            {
                int firstSector = Files[i].Track * SectorsOnTrack + Files[i].Sector;
                int lastSector = firstSector + Files[i].Size;
                if (diskAddress >= firstSector && diskAddress < lastSector) return Files[i];
            }
            return null;
        }

        public MList<DiskString> FindString(string str, int xor, int extendBy)
        {
            int last = Data.Length - str.Length;
            byte[] clip = new byte[str.Length];
            byte[] clip1 = new byte[str.Length + extendBy * 2];
            MList<DiskString> result = new MList<DiskString>();
            for (int i = 0; i < last; i++)
            {
                for (int j = 0; j < str.Length; j++)
                {
                    clip[j] = (byte)(Data[i + j] ^ xor);
                }
                string s0 = Encoding.ASCII.GetString(clip, 0, str.Length);
                if (s0.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    DiskString ds = new DiskString();
                    ds.Image = this;
                    ds.Track = i / SectorSize / SectorsOnTrack;
                    ds.Sector = i / SectorSize - ds.Track * SectorsOnTrack;
                    ds.Offset = i - ds.Track * SectorsOnTrack * SectorSize - ds.Sector * SectorSize;
                    ds.Xor = xor;
                    ds.File = GetFileByDiskAddress(ds.Track, ds.Sector);
                    int start = Math.Max(0, i - extendBy);
                    int end = Math.Min(i + str.Length + extendBy, Data.Length);
                    for (int j = start, k = 0; j < end; j++, k++)
                    {
                        clip1[k] = (byte)(Data[j] ^ xor);
                    }
                    ds.Value = Encoding.ASCII.GetString(clip1, 0, end - start);
                    result.Add(ds);
                }
            }
            return result;
        }

        public MList<DiskString> FindBytes(byte[] bytes)
        {
            int last = Data.Length - bytes.Length;
            MList<DiskString> result = new MList<DiskString>();
            for (int i = 0; i < last; i++)
            {
                for (int j = 0; j < bytes.Length; j++)
                {
                    if (bytes[j] != Data[i + j]) goto noMatch;
                }
                DiskString ds = new DiskString();
                ds.Image = this;
                ds.Track = i / SectorSize / SectorsOnTrack;
                ds.Sector = i / SectorSize - ds.Track * SectorsOnTrack;
                ds.Offset = i - ds.Track * SectorsOnTrack * SectorSize - ds.Sector * SectorSize;
                ds.File = GetFileByDiskAddress(ds.Track, ds.Sector);
                result.Add(ds);
                noMatch:;
            }
            return result;
        }

        public override object Clone()
        {
            TrDosImage image = new TrDosImage();
            image.Data = new byte[Data.Length];
            Data.CopyTo(image.Data, 0);
            image.Sectors = new SectorProcessResult[Sectors.Length];
            Sectors.CopyTo(image.Sectors, 0);
            image.Name = Name;
            if (Files != null) image.Files = new MList<FileData>(Files, true, true);
            image.Title = Title;
            image.Free8Sector = Free8Sector;
            image.FileCount8Sector = FileCount8Sector;
            image.DiskType = DiskType;
            image.DeletedFiles8Sector = DeletedFiles8Sector;
            image.FileName = FileName;
            return image;
        }
    }
}
