using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumArchiveReader
{
    public class IsDosImage : DiskImage
    {
        public IsDosImage(int sizeSectors, Map map) : base(1024, 5, sizeSectors, map)
        {
            map?.BuildMap(Data, Sectors);
        }

        public IsDosImage() : base(1024, 5, 0, null)
        {
            
        }

        public byte[] ToIsd(int minSizeSectors)
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
    }
}
