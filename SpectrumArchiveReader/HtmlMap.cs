using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumArchiveReader
{
    public class HtmlMap
    {
        public static string BuildMap(string title, string jsArray, string values, MList<HtmlMapPars> pars)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""generator"" content=""Spectrum Archive Reader"">");
            sb.AppendLine($"        <title>{title}</title>");
            sb.AppendLine(@"        <style>");
            for (int i = 0; i < pars.Cnt; i++)
            {
                sb.AppendLine($"            .{pars[i].Char} {{ background-color: {pars[i].BackgroundColor}; }}");
            }
            sb.AppendLine(@"
            .map td { width: 5px; height: 5px; min-width: 5px; min-height: 5px; }
            #statsTable tr { text-align: left; margin-top: 30px; }
            #statsTable { margin-top: 30px; }
            #legend div div { width: 15px; height: 15px; float: left; margin-right: 15px; }
            #legend { margin: 30px; }
            #tablediv table { border-spacing: 0px; }
        </style>
    </head>
    <body>
        <div id=""legend"" style=""display: none;"">");
            for (int i = 0; i < pars.Cnt; i++)
            {
                sb.AppendLine($@"            <div><div class=""{pars[i].Char}""></div>{pars[i].Description}</div>");
            }
            sb.AppendLine(@"        </div>
        <div id=""tablediv""><h1 align=""center"">Rendering HTML ...</h1></div>");
            string indent = "    ";
            sb.AppendLine(indent + indent + "<script>");
            sb.AppendLine(indent + indent + indent + jsArray);
            sb.AppendLine(indent + indent + indent + values);
            sb.AppendLine(indent + indent + indent + @"var s = """";
            for (var d = 0; d < disks.length; d++)
            {
                var pars = values[d];
                s += ""<div>"" + pars[0] + "" | Title: "" + pars[1] + "" | Size: "" + pars[2] + "" | Good: "" + pars[3] + "" | Bad: "" 
                    + pars[4] + "" | Unprocessed: "" + pars[5] + "" | Files: "" + pars[6] + "" | Damaged: "" + pars[7] + "" | Non-Zero: "" 
                    + pars[8] + ""</div>"";
                s += ""<table class=\""map\"">"";
                var sectors = disks[d];
                var trackCnt = sectors.length / 16;
                if (trackCnt * 16 < sectors.length) trackCnt++;
                for (var sec = 0; sec < 16; sec++)
                {
                    s += ""<tr>"";
                    for (var tr = 0; tr < trackCnt; tr++)
                    {
                        var sectorNum = tr * 16 + sec;
                        if (sectorNum >= sectors.length)
                        {
                            s += ""<td class=\""u\""></td>"";
                        }
                        else
                        {
                            s += ""<td class=\"""" + sectors[sectorNum] + ""\""></td>"";
                        }
                    }
                    s += ""</tr>"";
                }
                s += ""</table>"";
            }
            document.getElementById(""tablediv"").innerHTML = s;
            document.getElementById(""legend"").style.display = ""block"";
            var tdMouseMove = function(e)
            {
                var elem = document.elementFromPoint(e.clientX, e.clientY);
                if (elem.nodeName != ""TD"") return;
                elem.title = ""Track: "" + elem.cellIndex  + "" | Sector: "" + elem.parentNode.rowIndex;
            }
            var maps = document.querySelectorAll("".map"");
            for (var i = 0; i < maps.length; i++)
            {
                maps[i].onmousemove = tdMouseMove;
            }
        </script>");
            sb.AppendLine(indent + "</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }
    }

    public class HtmlMapPars
    {
        public char Char;
        public string BackgroundColor;
        public string Description;

        public HtmlMapPars(char symbol, string backgroundColor, string description)
        {
            Char = symbol;
            BackgroundColor = backgroundColor;
            Description = description;
        }
    }
}
