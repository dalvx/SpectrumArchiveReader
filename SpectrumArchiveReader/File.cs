using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumArchiveReader
{
    public class FileData : ICloneable
    {
        public string FileName;
        public char Extension;
        public int Track;
        public int Sector;
        public int Size;
        public int Start;
        public int Length;
        public int GoodSectors;
        public int BadSectors;
        public int UnprocessedSectors;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
