using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class Map2
    {
        public ChartArea ChartArea;
        public int FirstTrack;
        public int LastTrack;
        public DiskImage2 Image;
        public ImageStatsTable2 StatsTable;
        private int firstTrackOld;
        private int lastTrackOld;
        public int MaxTrack;
        public byte ZeroByte;
        public bool CanEditReadBounds = true;
        private Color backColor;
        public const int CellWidth = 6;
        public const int TrackHeight = 250;
        private const int headerHeight = 10;
        private const int stripHeight = 5;
        private const int upperBoundHeight = 1;
        private const int bottomBoundHeight = 1;
        public event EventHandler ReadBoundsChanged;

        public Label TrackL;
        public Label TrackLV;
        public Label SectorL;
        public Label SectorLV;
        public Label StatusL;
        public Label StatusLV;
        public Label SectorSizeL;
        public Label SectorSizeLV;
        public Label CylL;
        public Label CylLV;
        public Label HeadL;
        public Label HeadLV;
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
        private ToolStripMenuItem markSelectionAsUnscanned;
        private ToolStripMenuItem markAsGood;
        private ToolStripMenuItem markSelectionAsGood;
        private ToolStripMenuItem viewSectorContents;

        private int sectorIndex;
        private int track;
        private bool mapMouseLeaveIgnore;
        private bool selecting;

        public Map2(int maxTrack, DiskImage2 image, Control parent, Color backColor, ImageStatsTable2 statsTable)
        {
            StatsTable = statsTable;
            MaxTrack = maxTrack;
            ChartArea = new ChartArea(parent);
            this.backColor = backColor;
            Image = image;
            if (Image != null)
            {
                for (int i = 0; i < Image.Tracks.Cnt; i++)
                {
                    if (Image.Tracks[i] == null) continue;
                    image.Tracks.Data[i].MapModified = true;
                }
            }
            ChartArea.Size = new Size(MaxTrack * CellWidth, TrackHeight + headerHeight + upperBoundHeight + bottomBoundHeight);
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
            SectorSizeL = new Label() { Parent = parent, Left = 250, Top = 343, Text = "Size:", AutoSize = true };
            SectorSizeLV = new Label() { Parent = parent, Left = 280, Top = 343, Text = "...", AutoSize = true };
            CylL = new Label() { Parent = parent, Left = 310, Top = 343, Text = "Cyl:", AutoSize = true };
            CylLV = new Label() { Parent = parent, Left = 332, Top = 343, Text = "...", AutoSize = true };
            HeadL = new Label() { Parent = parent, Left = 355, Top = 343, Text = "Head:", AutoSize = true };
            HeadLV = new Label() { Parent = parent, Left = 390, Top = 343, Text = "...", AutoSize = true };
            UnprocessedLC = new Label() { Parent = parent, Left = 420, Top = 343, Width = 19, Height = 13, BackColor = Color.Wheat };
            UnprocessedLCL = new Label() { Parent = parent, Left = 445, Top = 343, Text = "Unprocessed", AutoSize = true };
            GoodLC = new Label() { Parent = parent, Left = 548, Top = 343, Width = 19, Height = 13, BackColor = Color.Green };
            GoodLCL = new Label() { Parent = parent, Left = 573, Top = 343, Text = "Good", AutoSize = true };
            ZeroLC = new Label() { Parent = parent, Left = 639, Top = 343, Width = 19, Height = 13, BackColor = Color.Gray };
            ZeroLCL = new Label() { Parent = parent, Left = 664, Top = 343, Text = "Zero", AutoSize = true };
            CrcErrorLC = new Label() { Parent = parent, Left = 722, Top = 343, Width = 19, Height = 13, BackColor = Color.FromArgb(175, 0, 0) };
            CrcErrorLCL = new Label() { Parent = parent, Left = 747, Top = 343, Text = "CRC Error", AutoSize = true };
            HeaderNotFoundLC = new Label() { Parent = parent, Left = 818, Top = 343, Width = 19, Height = 13, BackColor = Color.FromArgb(0, 0, 223) };
            HeaderNotFoundLCL = new Label() { Parent = parent, Left = 843, Top = 343, Text = "Header Not Found", AutoSize = true };
            ProcessingLC = new Label() { Parent = parent, Left = 960, Top = 343, Width = 19, Height = 13, BackColor = Color.Black };
            ProcessingLCL = new Label() { Parent = parent, Left = 985, Top = 343, Text = "Reading", AutoSize = true };

            contextMenu = new ContextMenuStrip();
            contextMenuTopItem = new ToolStripMenuItem() { Enabled = false };
            markAsUnprocessed = new ToolStripMenuItem();
            markSelectionAsUnprocessed = new ToolStripMenuItem();
            markSelectionAsUnscanned = new ToolStripMenuItem();
            markAsGood = new ToolStripMenuItem();
            markSelectionAsGood = new ToolStripMenuItem();
            viewSectorContents = new ToolStripMenuItem();
            contextMenu.SuspendLayout();
            //
            // contextMenuStrip1
            // 
            contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                contextMenuTopItem,
                markAsUnprocessed,
                markSelectionAsUnprocessed,
                markSelectionAsUnscanned,
                markAsGood,
                markSelectionAsGood,
                viewSectorContents
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
            // markSelectionAsUnscanned
            // 
            markSelectionAsUnscanned.Name = "markSelectionAsUnscanned";
            markSelectionAsUnscanned.Size = new System.Drawing.Size(239, 22);
            markSelectionAsUnscanned.Text = "Mark Selection As Unscanned";
            markSelectionAsUnscanned.Click += MarkSelectionAsUnscanned_Click;
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
            // 
            // viewSectorContents
            // 
            viewSectorContents.Name = "viewSectorContents";
            viewSectorContents.Size = new System.Drawing.Size(239, 22);
            viewSectorContents.Text = "View Sector Contents";
            viewSectorContents.Click += ViewSectorContents_Click;
            //\
            contextMenu.ResumeLayout(false);
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
            for (int i = 0; i < Image.Tracks.Cnt; i++)
            {
                if (Image.Tracks.Data[i] == null) continue;
                for (int j = 0; j < Image.Tracks.Data[i].Layout.Cnt; j++)
                {
                    MapCell newValue = Image.Tracks.Data[i].Layout.Data[j].MapCellValue & ~value;
                    if (newValue != Image.Tracks.Data[i].Layout.Data[j].MapCellValue)
                    {
                        Image.Tracks.Data[i].Layout.Data[j].MapCellValue = newValue;
                        Image.Tracks.Data[i].MapModified = true;
                    }
                }
            }
        }

        public void MarkSectorAsProcessing(int track, int sectorIndex)
        {
            ClearHighlight(MapCell.Processing);
            Image.Tracks.Data[track].Layout.Data[sectorIndex].MapCellValue |= MapCell.Processing;
            Image.Tracks.Data[track].MapModified = true;
        }

        public void Select(int firstTrack, int lastTrack)
        {
            FirstTrack = firstTrack;
            LastTrack = lastTrack;
            ClearHighlight(MapCell.Highlighted | MapCell.Hover);
            for (int i = firstTrack; i < lastTrack; i++)
            {
                if (Image.Tracks.Data[i] == null) continue;
                for (int j = 0; j < Image.Tracks.Data[i].Layout.Cnt; j++)
                {
                    Image.Tracks.Data[i].Layout.Data[j].MapCellValue |= MapCell.Highlighted;
                }
                Image.Tracks.Data[i].MapModified = true;
            }
        }

        public void MarkTrack(int track, MapCell mask)
        {
            TrackFormat tf = Image.Tracks.Data[track];
            for (int i = 0; i < tf.Layout.Cnt; i++)
            {
                tf.Layout.Data[i].MapCellValue |= mask;
            }
            tf.MapModified = true;
        }

        public void UnmarkTrack(int track, MapCell mask)
        {
            TrackFormat tf = Image.Tracks.Data[track];
            for (int i = 0; i < tf.Layout.Cnt; i++)
            {
                tf.Layout.Data[i].MapCellValue &= ~mask;
            }
            tf.MapModified = true;
        }

        public void GetTrackSectorByMousePosition(int x, int y, out int track, out int sectorIndex)
        {
            track = x >= 0 ? x / CellWidth : -1;
            if (track > MaxTrack) track = MaxTrack;
            sectorIndex = -1;
            if (track < 0 || Image == null || track >= Image.Tracks.Cnt || y < headerHeight) return;
            TrackFormat tf = Image?.Tracks[track];
            if (tf == null || tf.Layout.Cnt == 0) return;
            int position = y - headerHeight;
            for (int i = 0; i < tf.Layout.Cnt; i++)
            {
                if (position >= tf.Layout.Data[i].MapPoint1 && position < tf.Layout.Data[i].MapPoint2)
                {
                    sectorIndex = i;
                    return;
                }
            }
            return;
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
            for (int i = 0; i < Image.Tracks.Cnt; i++)
            {
                if (Image.Tracks.Data[i].MapModified)
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
                int top = headerHeight + upperBoundHeight;
                for (int track = 0; track < Image.Tracks.Cnt; track++)
                {
                    TrackFormat tf = Image.Tracks.Data[track];
                    if (!tf.MapModified) continue;
                    tf.MapModified = false;
                    if (tf.Scanning)
                    {
                        WinApi.SetDCBrushColor(DC, GetCellColor(MapCell.Scanning));
                        WinApi.Rectangle(DC, CellWidth * track, top, CellWidth * (track + 1) + 1, top + TrackHeight + 1);
                        continue;
                    }
                    if (tf.FormatName == TrackFormatName.Unscanned)
                    {
                        WinApi.SetDCBrushColor(DC, GetCellColor(MapCell.Unprocessed));
                        WinApi.Rectangle(DC, CellWidth * track, top, CellWidth * (track + 1) + 1, top + TrackHeight + 1);
                        continue;
                    }
                    double prevSectorEnd = 0;
                    double cumulativeTime = 0;
                    double spinTime = tf.SpinTime;
                    if (spinTime == 0) spinTime = TrackFormat.SpinTimeStandard;
                    bool gapPainted = true;
                    int prevColor = 0;
                    for (int j = 0; j < tf.Layout.Cnt; j++)
                    {
                        double sectorStart = j == 0 ? 0 : cumulativeTime + tf.Layout.Data[j].TimeMs - TrackFormat.NormalSectorHeaderSize / tf.BytesPerMs;
                        cumulativeTime += j == 0 ? 59 / tf.BytesPerMs : tf.Layout.Data[j].TimeMs;
                        double sectorEnd = sectorStart + (tf.Layout.Data[j].SizeBytes + TrackFormat.NormalSectorHeaderSize) / tf.BytesPerMs;
                        double prevSectorEndRem = prevSectorEnd;
                        prevSectorEnd = sectorEnd;
                        MapCell cell = tf.Layout.Data[j].MapCellValue;
                        if (j != 0)
                        {
                            int gapPoint1 = (int)Math.Round(prevSectorEndRem / spinTime * TrackHeight);
                            int gapPoint2 = (int)Math.Round(sectorStart / spinTime * TrackHeight);
                            gapPainted = gapPoint2 > gapPoint1;
                            if (gapPainted)
                            {
                                WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(Color.WhiteSmoke));
                                WinApi.Rectangle(DC, CellWidth * track, top + gapPoint1, CellWidth * (track + 1) + 1, top + gapPoint2 + 1);
                            }
                        }
                        int point1 = (int)Math.Round(sectorStart / spinTime * TrackHeight);
                        int point2 = (int)Math.Round(sectorEnd / spinTime * TrackHeight);
                        tf.Layout.Data[j].MapPoint1 = point1;
                        tf.Layout.Data[j].MapPoint2 = point2;
                        int color = GetCellColor(cell);
                        if (!gapPainted && color == prevColor) color += 0x00202020;
                        prevColor = color;
                        WinApi.SetDCBrushColor(DC, color);
                        if ((cell & MapCell.Hover) != 0)
                        {
                            IntPtr penX = WinApi.SelectObject(DC, penBlack);
                            WinApi.Rectangle(DC, CellWidth * track, top + point1, CellWidth * (track + 1), top + point2);
                            WinApi.SelectObject(DC, penX);
                        }
                        else
                        {
                            WinApi.Rectangle(DC, CellWidth * track, top + point1, CellWidth * (track + 1) + 1, top + point2 + 1);
                        }
                    }
                    int gapPoint1x = (int)Math.Round(prevSectorEnd / spinTime * TrackHeight);
                    WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(Color.WhiteSmoke));
                    WinApi.Rectangle(DC, CellWidth * track, top + gapPoint1x, CellWidth * (track + 1) + 1, top + TrackHeight + 1);
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

                // Upper and bottom lines painting.
                WinApi.Rectangle(DC, 0, headerHeight , MaxTrack * CellWidth + 1, headerHeight + upperBoundHeight);
                WinApi.Rectangle(DC, 0, headerHeight + upperBoundHeight + TrackHeight, MaxTrack * CellWidth + 1, headerHeight + upperBoundHeight + TrackHeight + bottomBoundHeight);
            }
            WinApi.SelectObject(DC, oldPen);
            WinApi.SelectObject(DC, oldBrush);
            WinApi.DeleteObject(penNull);
            WinApi.DeleteObject(penBlack);
            return true;
        }

        public void Repaint()
        {
            if (PaintMap()) ChartArea.Repaint();
        }

        private void ChartArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (Image == null) return;
            int track;
            int sectorIndex;
            GetTrackSectorByMousePosition(e.X, e.Y, out track, out sectorIndex);
            TrackFormat tf = (track >= 0 && track < Image.Tracks.Cnt) ? Image.Tracks[track] : null;
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
            if (sectorIndex >= 0)
            {
                int diskSector = tf.Layout.Data[sectorIndex].SectorNumber;
                SectorLV.Text = diskSector.ToString();
                StatusLV.Text = tf.Layout.Data[sectorIndex].ProcessResult.ToString();
                SectorSizeLV.Text = tf.Layout.Data[sectorIndex].SizeBytes.ToString();
                CylLV.Text = tf.Layout.Data[sectorIndex].Cylinder.ToString();
                HeadLV.Text = tf.Layout.Data[sectorIndex].Head.ToString();
                if (!selecting)
                {
                    ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                    tf.Layout.Data[sectorIndex].MapCellValue |= MapCell.Hover;
                    tf.MapModified = true;
                }
            }
            else
            {
                if (track >= 0 && track < Image.Tracks.Cnt)
                {
                    if (Image[track].FormatName == TrackFormatName.Unscanned)
                    {
                        StatusLV.Text = TrackFormatName.Unscanned.ToString();
                    }
                    else if (Image[track].Scanning)
                    {
                        StatusLV.Text = "Scanning";
                    }
                    else
                    {
                        StatusLV.Text = "";
                    }
                    ClearSectorInfo(false, false);
                }
                else
                {
                    ClearSectorInfo(false, true);
                }
                ClearHighlight(MapCell.Hover);
            }
            Repaint();
        }

        private void ClearSectorInfo(bool clearTrack, bool clearStatus)
        {
            if (clearTrack) TrackLV.Text = "";
            SectorLV.Text = "";
            if (clearStatus) StatusLV.Text = "";
            SectorSizeLV.Text = "";
            CylLV.Text = "";
            HeadLV.Text = "";
        }

        private void ChartArea_MouseUp(object sender, MouseEventArgs e)
        {
            selecting = false;
            ClearHighlight();
            Repaint();
        }

        private void ChartArea_MouseDown(object sender, MouseEventArgs e)
        {
            GetTrackSectorByMousePosition(e.X, e.Y, out track, out sectorIndex);
            if (e.Button == MouseButtons.Right)
            {
                ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                TrackFormat tf = Image.Tracks.Data[track];
                bool sectorSelected = sectorIndex >= 0 && sectorIndex < tf.Layout.Cnt && track >= 0 && track < MainForm.MaxTrack;
                if (sectorSelected)
                {
                    tf.Layout.Data[sectorIndex].MapCellValue |= MapCell.Hover;
                    tf.MapModified = true;
                    int diskSector = tf.Layout.Data[sectorIndex].SectorNumber;
                    contextMenuTopItem.Text = $"Track: {track} Sector: {diskSector}";
                }
                else
                {
                    contextMenuTopItem.Text = $"Track: {track}";
                }
                markAsUnprocessed.Enabled = sectorSelected;
                markAsGood.Enabled = sectorSelected;
                viewSectorContents.Enabled = sectorSelected && (tf[sectorIndex].ProcessResult == SectorProcessResult.Good || tf[sectorIndex].ProcessResult == SectorProcessResult.Bad);
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
            ClearSectorInfo(true, true);
            ClearHighlight(MapCell.Highlighted | MapCell.Hover);
            Repaint();
        }

        private void MarkSelectionAsGood_Click(object sender, EventArgs e)
        {
            Image.MarkSectorRange(FirstTrack, LastTrack, SectorProcessResult.Good, MapCell.Good);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkAsGood_Click(object sender, EventArgs e)
        {
            Image.MarkSector(track, sectorIndex, SectorProcessResult.Good, MapCell.Good);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkSelectionAsUnprocessed_Click(object sender, EventArgs e)
        {
            Image.MarkSectorRange(FirstTrack, LastTrack, SectorProcessResult.Unprocessed, MapCell.Unprocessed);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkSelectionAsUnscanned_Click(object sender, EventArgs e)
        {
            Image.MarkTrackRangeAsUnscanned(FirstTrack, LastTrack);
            Repaint();
            StatsTable?.Repaint();
        }

        private void MarkAsUnprocessed_Click(object sender, EventArgs e)
        {
            Image.MarkSectorRange(FirstTrack, LastTrack, SectorProcessResult.Unprocessed, MapCell.Unprocessed);
            Repaint();
            StatsTable?.Repaint();
        }

        private void ViewSectorContents_Click(object sender, EventArgs e)
        {
            SectorContentsForm form = new SectorContentsForm();
            form.Show(Image, track, sectorIndex);
        }

        private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ChartArea_MouseLeave(sender, e);
        }
    }
}
