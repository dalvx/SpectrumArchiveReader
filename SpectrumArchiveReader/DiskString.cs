using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SpectrumArchiveReader
{
    /// <summary>
    /// Строка на диске: положение, файл, содержимое строки, байт которым она заксорена.
    /// </summary>
    public class DiskString
    {
        public string Value;
        public int Track;
        public int Sector;
        public int Offset;
        public FileData File;
        public DiskImage Image;
        public int Xor;
        /// <summary>
        /// Расширенная строка (контекст): строка перед найденной, найденная, и строка после найденной.
        /// </summary>
        public string ExtString;

        public string ToHtmlTableRow()
        {
            return $"<tr><td>{Image.FileNameOnly.Substring(0, 4)}</td><td>{File?.FileName}</td><td>{File?.Extension}</td><td>{File?.Size}</td><td>{Track}</td><td>{Sector}</td><td>{Offset}</td><td>{Xor}</td><td>{HttpUtility.HtmlEncode(Value)}</td></tr>";
        }
    }
}
