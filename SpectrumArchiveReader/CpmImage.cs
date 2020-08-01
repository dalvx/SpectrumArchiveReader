using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumArchiveReader
{
    public class CpmImage : DiskImage
    {
        public CpmImage(int sizeSectors, Map map) : base(1024, 5, sizeSectors, map)
        {
            StandardFormat = TrackFormat.Cpm;
            ZeroByte = 0xE5;
            map?.BuildMap(Data, Sectors);
        }

        public CpmImage() : base(1024, 5, 0, null)
        {
            StandardFormat = TrackFormat.Cpm;
            ZeroByte = 0xE5;
        }

        public byte[] ToKdi(int minSizeSectors)
        {
            int size = Math.Max(FileSectorsSize, minSizeSectors);
            byte[] r = new byte[size * SectorSize];
            Array.Copy(Data, 0, r, 0, Math.Min(r.Length, Data.Length));
            for (int i = 0; i < size; i++)
            {
                if (i < Sectors.Length)
                {
                    switch (Sectors[i])
                    {
                        case SectorProcessResult.Bad:
                        case SectorProcessResult.NoHeader:
                        case SectorProcessResult.Unprocessed:
                            Fill(r, i * SectorSize, SectorSize, ZeroByte);
                            break;
                    }
                }
                else
                {
                    Fill(r, i * SectorSize, SectorSize, ZeroByte);
                }
            }
            return r;
        }
    }
}
