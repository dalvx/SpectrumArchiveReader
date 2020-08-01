using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class ImageStatsTable
    {
        public ChartArea ChartArea;
        public DiskImage Image;
        private Color backColor;
        private Color foreColor;
        private Font font = new Font("Microsoft Sans Serif", 8.25f);
        public string[] Headers = new string[]
        {
            "Имя: ",
            "Треков:",
            "Секторов:",
            "Размер файла образа:",
            "Обработанных секторов:",
            "Необработанных секторов: ",
            "Good:",
            "Bad:",
            "Каталог прочитан:",
            "Поврежденных файлов:",
            "Образ изменен:"
        };
        private int textHeight;
        private int nameCaptionWidth;
        private int maxCaptionWidth;
        private string yes = "Да";
        private string no = "Нет";
        private string tracks = " треков)";
        private string from = " из ";
        private StringBuilder[] oldValues;
        private StringBuilder[] cvalues;

        public ImageStatsTable(Control parent, Color backColor, Color foreColor)
        {
            ChartArea = new ChartArea(parent);
            this.backColor = backColor;
            this.foreColor = foreColor;
            oldValues = new StringBuilder[Headers.Length];
            cvalues = new StringBuilder[Headers.Length];
            for (int i = 0; i < oldValues.Length; i++)
            {
                oldValues[i] = new StringBuilder(64);
                cvalues[i] = new StringBuilder(64);
            }
            ChartArea.Size = new Size(289, 151);
        }

        public void SetPosition(int left, int top)
        {
            ChartArea.Left = left;
            ChartArea.Top = top;
        }

        private void BuildStrings()
        {
            for (int i = 0; i < cvalues.Length; i++)
            {
                cvalues[i].Length = 0;
            }
            if (Image == null) return;
            cvalues[0].Append(Image.Name);
            AppendInt(cvalues[1], Image.SizeTracks);
            AppendInt(cvalues[2], Image.SizeSectors);
            AppendInt(cvalues[3], Image.FileSectorsSize);
            cvalues[3].Append(' ');
            cvalues[3].Append('(');
            AppendInt(cvalues[3], (int)Math.Ceiling((double)Image.FileSectorsSize / Image.SectorsOnTrack));
            cvalues[3].Append(tracks);
            AppendInt(cvalues[4], Image.ProcessedSectors);
            AppendInt(cvalues[5], Image.UnprocessedSectors);
            AppendInt(cvalues[6], Image.GoodSectors);
            AppendInt(cvalues[7], Image.BadSectors);
            if (Image is TrDosImage)
            {
                TrDosImage image = (TrDosImage)Image;
                if (image.CatIsRead)
                {
                    cvalues[8].Append(yes);
                }
                else
                {
                    if (image.Sectors.Length >= 9)
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            if (Image.Sectors[i] != SectorProcessResult.Good) continue;
                            for (int u = 0; u < 9; u++)
                            {
                                if (image.Sectors[u] == SectorProcessResult.Good)
                                {
                                    if (cvalues[8].Length > 0)
                                    {
                                        cvalues[8].Append(',');
                                        cvalues[8].Append(' ');
                                    }
                                    AppendInt(cvalues[8], u);
                                }
                            }
                            goto skip;
                        }
                        cvalues[8].Append(no);
                        skip:;
                    }
                }
                AppendInt(cvalues[9], image.DamagedFiles);
                cvalues[9].Append(from);
                AppendInt(cvalues[9], image.FileCountUntil0);
            }
            cvalues[10].Append(Image.Modified ? yes : no);
        }

        public bool IsChanged()
        {
            for (int i = 0; i < cvalues.Length; i++)
            {
                if (cvalues[i] != oldValues[i]) return true;
            }
            return false;
        }

        private void CopyValues()
        {
            for (int i = 0; i < cvalues.Length; i++)
            {
                oldValues[i].Length = 0;
                oldValues[i].Append(cvalues[i]);
            }
        }

        public void Paint()
        {
            IntPtr DC = ChartArea.bDC;
            IntPtr oldBrush = WinApi.SelectObject(DC, WinApi.GetStockObject(WinApi.StockObjects.DC_BRUSH));
            IntPtr penNull = WinApi.CreatePen(WinApi.PS_NULL, 1, (uint)ColorTranslator.ToWin32(Color.White));
            IntPtr oldPen = WinApi.SelectObject(DC, penNull);
            WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(backColor));
            WinApi.Rectangle(DC, 0, 0, ChartArea.Width + 1, ChartArea.Height + 1);
            IntPtr hfont = font.ToHfont();
            IntPtr oldFont = WinApi.SelectObject(DC, hfont);
            WinApi.SetBkMode(DC, WinApi.BkMode.TRANSPARENT);
            WinApi.SetTextColor(DC, ColorTranslator.ToWin32(foreColor));
            if (textHeight == 0)
            {
                WinApi.RECT rect = new WinApi.RECT();
                WinApi.DrawTextW(DC, Headers[0], Headers[0].Length, ref rect, WinApi.DT_CALCRECT);
                nameCaptionWidth = rect.Width;
                textHeight = rect.Height;
                WinApi.DrawTextW(DC, Headers[5], Headers[5].Length, ref rect, WinApi.DT_CALCRECT);
                maxCaptionWidth = rect.Width + 10;
            }
            int last = Headers.Length;
            for (int i = 0; i < last; i++)
            {
                WinApi.TextOutW(DC, 0, i * textHeight, Headers[i], Headers[i].Length);
            }
            WinApi.TextOutW(DC, nameCaptionWidth, 0, cvalues[0], cvalues[0].Length);
            for (int i = 1; i < last; i++)
            {
                WinApi.TextOutW(DC, maxCaptionWidth, i * textHeight, cvalues[i], cvalues[i].Length);
            }
            WinApi.SelectObject(DC, oldPen);
            WinApi.SelectObject(DC, oldBrush);
            WinApi.SelectObject(DC, oldFont);
            WinApi.DeleteObject(penNull);
            WinApi.DeleteObject(hfont);
        }

        private void AppendInt(StringBuilder sb, int value)
        {
            int index = sb.Length;
            int v = 1000000000;
            bool wasNonZero = false;
            int len = 0;
            while (v != 0)
            {
                int d = value / v;
                v /= 10;
                if (d == 0 && !wasNonZero) continue;
                sb.Length++;
                sb[index] = (char)('0' + d);
                index++;
                value -= d * v * 10;
                wasNonZero = true;
                len++;
            }
            if (len == 0)
            {
                sb.Length++;
                sb[index] = '0';
            }
        }

        public void Repaint()
        {
            BuildStrings();
            if (IsChanged())
            {
                Paint();
                CopyValues();
            }
            ChartArea.Repaint();
        }
    }
}
