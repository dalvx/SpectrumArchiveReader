using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumArchiveReader
{
    public class IsDosImage : DiskImage
    {
        public IsDosImage(int sizeSectors, Map map) : base(1024, 5, sizeSectors, map)
        {
            StandardFormat = TrackFormat.IsDos;
            map?.BuildMap(Data, Sectors);
        }

        public IsDosImage() : base(1024, 5, 0, null)
        {
            StandardFormat = TrackFormat.IsDos;
        }
    }
}
