using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class Map
    {
        public ChartArea ChartArea;
        public int FirstTrack;
        public int LastTrack;
        public DiskImage Image;
        public ImageStatsTable StatsTable;
        private int firstTrackOld;
        private int lastTrackOld;
        public const int CellWidth = 6;
        public const int CellHeight = 6;
        public int MaxTrack;
        public int SectorsOnTrack;
        public int SectorSize;
        public byte ZeroByte;
        public bool CanEditReadBounds = true;
        public MapCell[] WorkMap;
        private MapCell[] oldMap;
        private Color backColor;
        private const int headerHeight = 10;
        private const int stripHeight = 5;
        private int oldSectorArraySize;
        public event EventHandler ReadBoundsChanged;
        private bool sizeChanged;

        public Label TrackL;
        public Label TrackLV;
        public Label SectorL;
        public Label SectorLV;
        public Label StatusL;
        public Label StatusLV;
        public Label FileL;
        public Label FileLV;
        public Label ExtenstionLV;
        public Label UnprocessedLC;
        public Label UnprocessedLCL;
        public Label GoodLC;
        public Label GoodLCL;
        public Label ZeroLC;
        public Label ZeroLCL;
        public Label CrcErrorLC;
        public Label CrcErrorLCL;
        public Label HeaderNotFoundLC;
        public Label HeaderNotFoundLCL;
        public Label ProcessingLC;
        public Label ProcessingLCL;

        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem contextMenuTopItem;
        private ToolStripMenuItem markAsUnprocessed;
        private ToolStripMenuItem markSelectionAsUnprocessed;
        private ToolStripMenuItem markAsGood;
        private ToolStripMenuItem markSelectionAsGood;

        private int sector;
        private int track;
        private bool mapMouseLeaveIgnore;
        private bool selecting;

        public Map(int maxTrack, int sectorSize, int sectorsOnTrack, Control parent, Color backColor, ImageStatsTable statsTable)
        {
            StatsTable = statsTable;
            MaxTrack = maxTrack;
            SectorSize = sectorSize;
            SectorsOnTrack = sectorsOnTrack;
            WorkMap = new MapCell[MaxTrack * SectorsOnTrack];
            oldMap = new MapCell[MaxTrack * SectorsOnTrack];
            ChartArea = new ChartArea(parent);
            this.backColor = backColor;
            for (int i = 0; i < oldMap.Length; i++)
            {
                oldMap[i] = (MapCell)0xFFFF;
            }
            ChartArea.Size = new Size(MaxTrack * CellWidth, SectorsOnTrack * CellHeight + headerHeight);
            ChartArea.MouseDoubleClick += ChartArea_MouseDoubleClick;
            ChartArea.MouseMove += ChartArea_MouseMove;
            ChartArea.MouseLeave += ChartArea_MouseLeave;
            ChartArea.MouseDown += ChartArea_MouseDown;
            ChartArea.MouseUp += ChartArea_MouseUp;

            TrackL = new Label() { Parent = parent, Left = 0, Top = 343, Text = "Track:", AutoSize = true };
            TrackLV = new Label() { Parent = parent, Left = 35, Top = 343, Text = "...", AutoSize = true };
            SectorL = new Label() { Parent = parent, Left = 60, Top = 343, Text = "Sector:", AutoSize = true };
            SectorLV = new Label() { Parent = parent, Left = 98, Top = 343, Text = "...", AutoSize = true };
            StatusL = new Label() { Parent = parent, Left = 130, Top = 343, Text = "Status:", AutoSize = true };
            StatusLV = new Label() { Parent = parent, Left = 170, Top = 343, Text = "...", AutoSize = true };
            FileL = new Label() { Parent = parent, Left = 250, Top = 343, Text = "File:", AutoSize = true };
            FileLV = new Label() { Parent = parent, Left = 275, Top = 343, Text = "...", AutoSize = true };
            ExtenstionLV = new Label() { Parent = parent, Left = 340, Top = 343, Text = "...", AutoSize = true };
            UnprocessedLC = new Label() { Parent = parent, Left = 380, Top = 343, Width = 19, Height = 13, BackColor = Color.Wheat };
            UnprocessedLCL = new Label() { Parent = parent, Left = 405, Top = 343, Text = "Unprocessed", AutoSize = true };
            GoodLC = new Label() { Parent = parent, Left = 508, Top = 343, Width = 19, Height = 13, BackColor = Color.Green };
            GoodLCL = new Label() { Parent = parent, Left = 533, Top = 343, Text = "Good", AutoSize = true };
            ZeroLC = new Label() { Parent = parent, Left = 599, Top = 343, Width = 19, Height = 13, BackColor = Color.Gray };
            ZeroLCL = new Label() { Parent = parent, Left = 624, Top = 343, Text = "Zero", AutoSize = true };
            CrcErrorLC = new Label() { Parent = parent, Left = 682, Top = 343, Width = 19, Height = 13, BackColor = Color.FromArgb(175, 0, 0) };
            CrcErrorLCL = new Label() { Parent = parent, Left = 707, Top = 343, Text = "CRC Error", AutoSize = true };
            HeaderNotFoundLC = new Label() { Parent = parent, Left = 778, Top = 343, Width = 19, Height = 13, BackColor = Color.FromArgb(0, 0, 223) };
            HeaderNotFoundLCL = new Label() { Parent = parent, Left = 803, Top = 343, Text = "Header Not Found", AutoSize = true };
            ProcessingLC = new Label() { Parent = parent, Left = 920, Top = 343, Width = 19, Height = 13, BackColor = Color.Black };
            ProcessingLCL = new Label() { Parent = parent, Left = 945, Top = 343, Text = "Reading", AutoSize = true };

            contextMenu = new ContextMenuStrip();
            contextMenuTopItem = new ToolStripMenuItem() { Enabled = false };
            markAsUnprocessed = new ToolStripMenuItem();
            markSelectionAsUnprocessed = new ToolStripMenuItem();
            markAsGood = new ToolStripMenuItem();
            markSelectionAsGood = new ToolStripMenuItem();
            contextMenu.SuspendLayout();
            //
            // contextMenuStrip1
            // 
            contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                contextMenuTopItem,
                markAsUnprocessed,
                markSelectionAsUnprocessed,
                markAsGood,
                markSelectionAsGood
            });
            contextMenu.Name = "contextMenu";
            contextMenu.Size = new System.Drawing.Size(240, 70);
            contextMenu.Closed += ContextMenu_Closed;
            // 
            // markAsUnprocessed
            // 
            markAsUnprocessed.Name = "markAsUnprocessed";
            markAsUnprocessed.Size = new System.Drawing.Size(239, 22);
            markAsUnprocessed.Text = "Mark Sector As Unprocessed";
            markAsUnprocessed.Click += MarkAsUnprocessed_Click;
            // 
            // markSelectionAsUnprocessed
            // 
            markSelectionAsUnprocessed.Name = "markSelectionAsUnprocessed";
            markSelectionAsUnprocessed.Size = new System.Drawing.Size(239, 22);
            markSelectionAsUnprocessed.Text = "Mark Selection As Unprocessed";
            markSelectionAsUnprocessed.Click += MarkSelectionAsUnprocessed_Click;
            // 
            // markAsGood
            // 
            markAsGood.Name = "markAsGood";
            markAsGood.Size = new System.Drawing.Size(239, 22);
            markAsGood.Text = "Mark Sector As Good";
            markAsGood.Click += MarkAsGood_Click;
            // 
            // markSelectionAsGood
            // 
            markSelectionAsGood.Name = "markSelectionAsGood";
            markSelectionAsGood.Size = new System.Drawing.Size(239, 22);
            markSelectionAsGood.Text = "Mark Selection As Good";
            markSelectionAsGood.Click += MarkSelectionAsGood_Click;
            //\
            contextMenu.ResumeLayout(false);
        }

        public void ChangeSize(int sectorSize, int sectorsOnTrack)
        {
            SectorSize = sectorSize;
            SectorsOnTrack = sectorsOnTrack;
            WorkMap = new MapCell[MaxTrack * SectorsOnTrack];
            oldMap = new MapCell[MaxTrack * SectorsOnTrack];
            firstTrackOld = -1;
            for (int i = 0; i < oldMap.Length; i++)
            {
                oldMap[i] = (MapCell)0xFFFF;
            }
            sizeChanged = true;
        }

        private void ChartArea_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!CanEditReadBounds) return;
            FirstTrack = 0;
            LastTrack = MainForm.MaxTrack;
            Repaint();
            ReadBoundsChanged?.Invoke(this, null);
        }

        public void SetPosition(int left, int top)
        {
            ChartArea.Left = left;
            ChartArea.Top = top;
        }

        public void ClearHighlight(MapCell value = MapCell.Highlighted)
        {
            for (int i = 0; i < WorkMap.Length; i++)
            {
                WorkMap[i] = WorkMap[i] & ~value;
            }
        }

        public void BuildMap(byte[] data, SectorProcessResult[] sectors)
        {
            int i;
            for (i = 0; i < sectors.Length; i++)
            {
                switch (sectors[i])
                {
                    case SectorProcessResult.Unprocessed:
                        WorkMap[i] = MapCell.Unprocessed;
                        break;

                    case SectorProcessResult.Good:
                        WorkMap[i] = DiskImage.AllBytes(data, i * SectorSize, SectorSize, ZeroByte) ? MapCell.Zero : MapCell.Good;
                        break;

                    case SectorProcessResult.Bad:
                        WorkMap[i] = MapCell.CrcError;
                        break;

                    case SectorProcessResult.NoHeader:
                        WorkMap[i] = MapCell.NoHeader;
                        break;
                }
            }
            for (; i < WorkMap.Length; i++)
            {
                WorkMap[i] = 0;
            }
            oldSectorArraySize = sectors.Length;
        }

        public void ModifySectors(int index, int length, byte[] data, SectorProcessResult[] sectors)
        {
            int last = Math.Min(index + length, sectors.Length);
            for (int sectorNum = index; sectorNum < last; sectorNum++)
            {
                MapCell cell = WorkMap[sectorNum] & (MapCell.Highlighted | MapCell.Hover);
                switch (sectors[sectorNum])
                {
                    case SectorProcessResult.Unprocessed:
                        WorkMap[sectorNum] = cell | MapCell.Unprocessed;
                        break;

                    case SectorProcessResult.Good:
                        WorkMap[sectorNum] = cell | (DiskImage.AllBytes(data, sectorNum * SectorSize, SectorSize, ZeroByte) ? MapCell.Zero : MapCell.Good);
                        break;

                    case SectorProcessResult.Bad:
                        WorkMap[sectorNum] = cell | MapCell.CrcError;
                        break;

                    case SectorProcessResult.NoHeader:
                        WorkMap[sectorNum] = cell | MapCell.NoHeader;
                        break;
                }
            }
            for (int i = sectors.Length; i < oldSectorArraySize; i++)
            {
                WorkMap[i] = 0;
            }
            oldSectorArraySize = sectors.Length;
        }

        public void Select(int firstTrack, int lastTrack)
        {
            FirstTrack = firstTrack;
            LastTrack = lastTrack;
            ClearHighlight(MapCell.Highlighted | MapCell.Hover);
            for (int i = firstTrack * SectorsOnTrack, last = lastTrack * SectorsOnTrack; i < last; i++)
            {
                WorkMap[i] |= MapCell.Highlighted;
            }
        }

        public void HighlightFile(int sectorNumber, int size)
        {
            for (int i = sectorNumber, last = sectorNumber + size; i < last; i++)
            {
                WorkMap[i] |= MapCell.Highlighted;
            }
        }

        public void MarkAsProcessing(int sectorNumber, int count = 1)
        {
            for (int i = sectorNumber, last = i + count; i < last; i++)
            {
                WorkMap[i] |= MapCell.Processing;
            }
        }

        public void MarkAsScanning(int index, int length = 1)
        {
            for (int i = index, last = i + length; i < last; i++)
            {
                WorkMap[i] |= MapCell.Scanning;
            }
        }

        public void GetTrackSectorByMousePosition(int x, int y, out int track, out int sector)
        {
            track = x >= 0 ? x / CellWidth : -1;
            sector = y >= headerHeight ? (y - headerHeight) / CellHeight : -1;
        }

        private int GetCellColor(MapCell cell)
        {
            if (cell == 0) return ColorTranslator.ToWin32(backColor);
            int color = 0;
            if ((cell & MapCell.Zero) != 0) color = ColorTranslator.ToWin32(Color.Gray);
            if ((cell & MapCell.CrcError) != 0) unchecked { color = ColorTranslator.ToWin32(Color.FromArgb((int)0xFFAF0000)); }
            if ((cell & MapCell.NoHeader) != 0) unchecked { color = ColorTranslator.ToWin32(Color.FromArgb((int)0xFF0000DF)); }
            if ((cell & MapCell.Good) != 0) color = ColorTranslator.ToWin32(Color.Green);
            if ((cell & MapCell.Unprocessed) != 0) color = ColorTranslator.ToWin32(Color.Wheat);
            if ((cell & MapCell.Processing) != 0) unchecked { color = ColorTranslator.ToWin32(Color.Black); }
            if ((cell & MapCell.Scanning) != 0) color = ColorTranslator.ToWin32(Color.Cyan);
            if ((cell & MapCell.Highlighted) != 0) color += 0x00202020;
            return color;
        }

        private bool PaintMap()
        {
            bool paintMap = false;
            for (int i = 0; i < WorkMap.Length; i++)
            {
                if (WorkMap[i] != oldMap[i])
                {
                    paintMap = true;
                    break;
                }
            }
            bool paintHeader = FirstTrack != firstTrackOld || LastTrack != lastTrackOld;
            if (!paintMap && !paintHeader) return false;
            IntPtr DC = ChartArea.bDC;
            IntPtr penNull = WinApi.CreatePen(WinApi.PS_NULL, 1, (uint)ColorTranslator.ToWin32(Color.White));
            IntPtr oldBrush = WinApi.SelectObject(DC, WinApi.GetStockObject(WinApi.StockObjects.DC_BRUSH));
            IntPtr oldPen = WinApi.SelectObject(DC, penNull);
            IntPtr penBlack = WinApi.CreatePen(WinApi.PS_SOLID, 1, (uint)ColorTranslator.ToWin32(Color.Black));
            if (paintMap)
            {
                for (int i = 0; i < WorkMap.Length; i++)
                {
                    if (WorkMap[i] == oldMap[i]) continue;
                    oldMap[i] = WorkMap[i];
                    WinApi.SetDCBrushColor(DC, GetCellColor(WorkMap[i]));
                    int track = i / SectorsOnTrack;
                    int sector = i % SectorsOnTrack;
                    if ((WorkMap[i] & MapCell.Hover) != 0)
                    {
                        IntPtr penX = WinApi.SelectObject(DC, penBlack);
                        WinApi.Rectangle(DC, CellWidth * track, headerHeight + sector * CellHeight, CellWidth * (track + 1), headerHeight + (sector + 1) * CellHeight);
                        WinApi.SelectObject(DC, penX);
                    }
                    else
                    {
                        WinApi.Rectangle(DC, CellWidth * track, headerHeight + sector * CellHeight, CellWidth * (track + 1) + 1, headerHeight + (sector + 1) * CellHeight + 1);
                    }
                }
            }
            if (paintHeader)
            {
                firstTrackOld = FirstTrack;
                lastTrackOld = LastTrack;
                WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(backColor));
                WinApi.Rectangle(DC, 0, stripHeight, MaxTrack * CellWidth + 1, headerHeight + 1);
                if (FirstTrack > 0) WinApi.Rectangle(DC, 0, 0, FirstTrack * CellWidth + 1, stripHeight + 1);
                if (LastTrack < MaxTrack) WinApi.Rectangle(DC, LastTrack * CellWidth, 0, MaxTrack * CellWidth + 1, stripHeight + 1);
                WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(Color.Black));
                WinApi.Rectangle(DC, FirstTrack * CellWidth, 0, LastTrack * CellWidth + 1, stripHeight + 1);
                WinApi.Rectangle(DC, 160 * CellWidth, stripHeight + 1, 172 * CellWidth + 1, headerHeight);
            }
            WinApi.SelectObject(DC, oldPen);
            WinApi.SelectObject(DC, oldBrush);
            WinApi.DeleteObject(penNull);
            WinApi.DeleteObject(penBlack);
            return true;
        }

        public void Repaint()
        {
            if (sizeChanged)
            {
                ChartArea.Size = new Size(MaxTrack * CellWidth, SectorsOnTrack * CellHeight + headerHeight);
                sizeChanged = false;
            }
            if (PaintMap()) ChartArea.Repaint();
        }

        private void ChartArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (Image == null) return;
            int track;
            int sector;
            GetTrackSectorByMousePosition(e.X, e.Y, out track, out sector);
            int sectorNumber = track * SectorsOnTrack + sector;
            bool sectorSelected = sector >= 0 && sector < SectorsOnTrack && track >= 0 && track < MainForm.MaxTrack && sectorNumber < Image.SizeSectors;
            if (selecting)
            {
                int minTrack = Math.Min(track, this.track);
                minTrack = Math.Max(0, minTrack);
                int maxTrack = Math.Max(track, this.track);
                maxTrack = Math.Min(maxTrack, MainForm.MaxTrack);
                if (maxTrack == minTrack) maxTrack++;
                if (maxTrack > MainForm.MaxTrack)
                {
                    maxTrack--;
                    minTrack--;
                    if (minTrack < 0) minTrack = 0;
                }
                Select(minTrack, maxTrack);
                ReadBoundsChanged?.Invoke(this, null);
            }
            TrackLV.Text = track.ToString();
            if (sectorSelected)
            {
                int diskSector = Image.StandardFormat.Layout.Data[sector].SectorNumber;
                SectorLV.Text = diskSector.ToString();
                StatusLV.Text = Image.Sectors[sectorNumber].ToString();
                if (Image is TrDosImage)
                {
                    FileData file = ((TrDosImage)Image).GetFileByDiskAddress(track, sector);
                    if (file != null)
                    {
                        FileLV.Text = file.FileName;
                        ExtenstionLV.Text = file.Extension.ToString();
                    }
                    else
                    {
                        FileLV.Text = "";
                        ExtenstionLV.Text = "";
                    }
                    if (!selecting)
                    {
                        ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                        WorkMap[sectorNumber] |= MapCell.Hover;
                        if (file != null)
                        {
                            int sn = file.Track * SectorsOnTrack + file.Sector;
                            HighlightFile(sn, Math.Min(file.Size, Image.SizeSectors - sn));
                        }
                    }
                }
                else if (!selecting)
                {
                    ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                    WorkMap[sectorNumber] |= MapCell.Hover;
                }
            }
            else
            {
                ClearTrackSectorFileName(false);
                ClearHighlight(MapCell.Hover);
            }
            Repaint();
        }

        private void ClearTrackSectorFileName(bool clearTrack)
        {
            if (clearTrack) TrackLV.Text = "";
            SectorLV.Text = "";
            StatusLV.Text = "";
            FileLV.Text = "";
            ExtenstionLV.Text = "";
        }

        private void ChartArea_MouseUp(object sender, MouseEventArgs e)
        {
            selecting = false;
            ClearHighlight();
            Repaint();
        }

        private void ChartArea_MouseDown(object sender, MouseEventArgs e)
        {
            GetTrackSectorByMousePosition(e.X, e.Y, out track, out sector);
            if (e.Button == MouseButtons.Right)
            {
                ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                int sectorNumber = track * SectorsOnTrack + sector;
                bool sectorSelected = sector >= 0 && sector < SectorsOnTrack && track >= 0 && track < MainForm.MaxTrack && sectorNumber < Image.SizeSectors;
                if (sectorSelected)
                {
                    WorkMap[sectorNumber] |= MapCell.Hover;
                    int diskSector = Image.StandardFormat.Layout.Data[sector].SectorNumber;
                    contextMenuTopItem.Text = $"Track: {track} Sector: {diskSector}";
                    markAsUnprocessed.Enabled = true;
                    markAsGood.Enabled = true;
                }
                else
                {
                    contextMenuTopItem.Text = $"Track: {track}";
                    markAsUnprocessed.Enabled = false;
                    markAsGood.Enabled = false;
                }
                Repaint();
                mapMouseLeaveIgnore = true;
                contextMenu.Show(ChartArea, new Point(e.X, e.Y));
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            ClearHighlight();
            if (CanEditReadBounds) selecting = true;
            ChartArea_MouseMove(sender, e);
        }

        private void ChartArea_MouseLeave(object sender, EventArgs e)
        {
            if (mapMouseLeaveIgnore)
            {
                mapMouseLeaveIgnore = false;
                return;
            }
            ClearTrackSectorFileName(true);
            ClearHighlight(MapCell.Highlighted | MapCell.Hover);
            Repaint();
        }

        private void MarkSelectionAsGood_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Good, FirstTrack * SectorsOnTrack, (LastTrack - FirstTrack) * SectorsOnTrack);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkAsGood_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Good, track * SectorsOnTrack + sector);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkSelectionAsUnprocessed_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Unprocessed, FirstTrack * SectorsOnTrack, (LastTrack - FirstTrack) * SectorsOnTrack);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkAsUnprocessed_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Unprocessed, track * SectorsOnTrack + sector);
            Repaint();
            StatsTable?.Repaint();
        }

        private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ChartArea_MouseLeave(sender, e);
        }
    }

    [Flags]
    public enum MapCell
    {
        Unprocessed = 0x01,
        Good = 0x02,
        NoHeader = 0x04,
        CrcError = 0x08,
        Zero = 0x10,
        Highlighted = 0x20,
        Hover = 0x40,
        Processing = 0x80,
        Scanning = 0x0100
    }
}
