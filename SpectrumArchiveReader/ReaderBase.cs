using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public abstract class ReaderBase
    {
        public int SectorsOnTrack;
        public int SectorSize;
        public Control Parent;
        protected GroupBox readSide;
        protected RadioButton side0;
        protected RadioButton side1;
        protected RadioButton sideBoth;
        protected Label sectorReadAttemptsL;
        protected TextBox sectorReadAttempts;
        protected Label trackFromL;
        protected TextBox trackFrom;
        protected Label trackToL;
        protected TextBox trackTo;
        protected GroupBox imageGB;
        protected Button newImage;
        protected Button loadImage;
        protected Button saveImage;
        protected Button showCatalogue;
        protected Button showCatFromTrack;
        protected Button setSize;
        protected Button mergeImage;
        protected Button readForward;
        protected Button readBackward;
        protected Button readRandomSectors;
        protected Button abortButton;
        protected Button readCatalogue;
        protected Label trackL;
        protected Label trackLV;
        protected Label sectorL;
        protected Label sectorLV;
        protected Label statusL;
        protected Label statusLV;
        protected Label fileL;
        protected Label fileLV;
        protected Label extenstionLV;
        protected Label unprocessedLC;
        protected Label unprocessedLCL;
        protected Label goodLC;
        protected Label goodLCL;
        protected Label zeroLC;
        protected Label zeroLCL;
        protected Label crcErrorLC;
        protected Label crcErrorLCL;
        protected Label headerNotFoundLC;
        protected Label headerNotFoundLCL;
        protected Label processingLC;
        protected Label processingLCL;
        protected Label dataRateL;
        protected ComboBox dataRate;
        protected ContextMenuStrip contextMenu;
        protected ToolStripMenuItem contextMenuTopItem;
        protected ToolStripMenuItem markAsUnprocessed;
        protected ToolStripMenuItem markSelectionAsUnprocessed;
        protected ToolStripMenuItem markAsGood;
        protected ToolStripMenuItem markSelectionAsGood;
        protected bool selecting;
        protected bool aborted;
        protected Map map;
        protected ImageStatsTable stats;
        public DiskImage Image;
        protected DiskReader diskReader;
        protected int newImageSize = 160;
        public DiskReaderParams Params;
        protected static DataRate[] dataRateArray = new DataRate[] { DataRate.FD_RATE_250K, DataRate.FD_RATE_300K, DataRate.FD_RATE_500K, DataRate.FD_RATE_1M };
        protected string newImageName = "New Image";
        public event EventHandler OperationStarted;
        public event EventHandler OperationCompleted;
        private bool processing;
        public bool Processing { get { return processing; } }
        private bool enabled = true;
        public bool Enabled
        {
            get { return enabled; }

            set
            {
                enabled = value;
                SetEnabled();
            }
        }
        private int sector;
        private int track;
        private bool mapMouseLeaveIgnore;

        public ReaderBase(Control parent, int sectorSize, int sectorsOnTrack, DataRate defaultDataRate)
        {
            Parent = parent;
            SectorSize = sectorSize;
            SectorsOnTrack = sectorsOnTrack;
            parent.SuspendLayout();
            Params = new DiskReaderParams() { SectorSize = sectorSize, SectorsOnTrack = sectorsOnTrack };
            readSide = new GroupBox() { Parent = parent, Left = 202, Top = 9, Width = 105, Height = 83, Text = "Read Side" };
            side0 = new RadioButton() { Parent = readSide, Left = 6, Top = 19, Text = "Side 0", AutoSize = true };
            side1 = new RadioButton() { Parent = readSide, Left = 6, Top = 39, Text = "Side 1", AutoSize = true };
            sideBoth = new RadioButton() { Parent = readSide, Left = 6, Top = 60, Text = "Both", AutoSize = true, Checked = true };
            sectorReadAttemptsL = new Label() { Parent = parent, Left = 312, Top = 3, Text = "Sector Read Attempts", AutoSize = true };
            sectorReadAttempts = new TextBox() { Parent = parent, Left = 315, Top = 19, Text = "1" };
            trackFromL = new Label() { Parent = parent, Left = 313, Top = 44, Text = "Track From", AutoSize = true };
            trackToL = new Label() { Parent = parent, Left = 375, Top = 44, Text = "Track To", AutoSize = true };
            trackFrom = new TextBox() { Parent = parent, Left = 315, Top = 61, Width = 56, Text = "0" };
            trackTo = new TextBox() { Parent = parent, Left = 378, Top = 61, Width = 56, Text = MainForm.MaxTrack.ToString() };
            imageGB = new GroupBox() { Parent = parent, Left = 651, Top = 5, Width = 381, Height = 213, Text = "Образ" };
            newImage = new Button() { Parent = imageGB, Left = 300, Top = 37, Width = 75, Height = 23, Text = "New" };
            loadImage = new Button() { Parent = imageGB, Left = 300, Top = 66, Width = 75, Height = 23, Text = "Load" };
            saveImage = new Button() { Parent = imageGB, Left = 300, Top = 94, Width = 75, Height = 23, Text = "Save" };
            showCatalogue = new Button() { Parent = imageGB, Left = 10, Top = 157, Width = 96, Height = 23, Text = "Show Catalogue" };
            showCatFromTrack = new Button() { Parent = imageGB, Left = 113, Top = 157, Width = 128, Height = 23, Text = "Show Cat From Track" };
            setSize = new Button() { Parent = imageGB, Left = 10, Top = 184, Width = 75, Height = 23, Text = "Set Size" };
            mergeImage = new Button() { Parent = imageGB, Left = 113, Top = 184, Width = 75, Height = 23, Text = "Merge" };
            readForward = new Button() { Parent = parent, Left = 549, Top = 10, Width = 96, Height = 23, Text = "Read Forward" };
            readBackward = new Button() { Parent = parent, Left = 549, Top = 36, Width = 96, Height = 23, Text = "Read Backward" };
            readRandomSectors = new Button() { Parent = parent, Left = 511, Top = 72, Width = 134, Height = 23, Text = "Read Random Sectors" };
            abortButton = new Button() { Parent = parent, Left = 568, Top = 193, Width = 75, Height = 23, Text = "Abort" };
            readCatalogue = new Button() { Parent = parent, Left = 6, Top = 50, Width = 144, Height = 23, Text = "Read Catalogue" };
            dataRateL = new Label() { Parent = parent, Left = 3, Top = 10, Text = "Data Rate", AutoSize = true };
            dataRate = new ComboBox() { Parent = parent, Left = 6, Top = 26, Width = 121, Height = 21, DropDownStyle = ComboBoxStyle.DropDownList };
            int defaultDataRateIndex = 0;
            for (int i = 0; i < dataRateArray.Length; i++)
            {
                dataRate.Items.Add(dataRateArray[i].ToString().Replace("FD_RATE_" , ""));
                if (dataRateArray[i] == defaultDataRate) defaultDataRateIndex = i;
            }
            dataRate.SelectedIndex = defaultDataRateIndex;
            map = new Map(MainForm.MaxTrack, SectorSize, SectorsOnTrack, parent, Color.White);
            map.SetPosition(0, 227);
            map.DoubleClick += Map_HeaderDoubleClick;
            map.ChartArea.MouseMove += ChartArea_MouseMove;
            map.ChartArea.MouseLeave += ChartArea_MouseLeave;
            map.ChartArea.MouseDown += ChartArea_MouseDown;
            map.ChartArea.MouseUp += ChartArea_MouseUp;
            map.TrackFrom = 0;
            map.TrackTo = MainForm.MaxTrack;
            map.Repaint();
            stats = new ImageStatsTable(imageGB, SystemColors.Window, SystemColors.ControlText);
            stats.SetPosition(6, 16);
            stats.Repaint();

            trackL = new Label() { Parent = parent, Left = 0, Top = 343, Text = "Track:", AutoSize = true };
            trackLV = new Label() { Parent = parent, Left = 35, Top = 343, Text = "...", AutoSize = true };
            sectorL = new Label() { Parent = parent, Left = 60, Top = 343, Text = "Sector:", AutoSize = true };
            sectorLV = new Label() { Parent = parent, Left = 98, Top = 343, Text = "...", AutoSize = true };
            statusL = new Label() { Parent = parent, Left = 130, Top = 343, Text = "Status:", AutoSize = true };
            statusLV = new Label() { Parent = parent, Left = 170, Top = 343, Text = "...", AutoSize = true };
            fileL = new Label() { Parent = parent, Left = 250, Top = 343, Text = "File:", AutoSize = true };
            fileLV = new Label() { Parent = parent, Left = 275, Top = 343, Text = "...", AutoSize = true };
            extenstionLV = new Label() { Parent = parent, Left = 340, Top = 343, Text = "...", AutoSize = true };
            unprocessedLC = new Label() { Parent = parent, Left = 380, Top = 343, Width = 19, Height = 13, BackColor = Color.Wheat };
            unprocessedLCL = new Label() { Parent = parent, Left = 405, Top = 343, Text = "Unprocessed", AutoSize = true };
            goodLC = new Label() { Parent = parent, Left = 508, Top = 343, Width = 19, Height = 13, BackColor = Color.Green };
            goodLCL = new Label() { Parent = parent, Left = 533, Top = 343, Text = "Good", AutoSize = true };
            zeroLC = new Label() { Parent = parent, Left = 599, Top = 343, Width = 19, Height = 13, BackColor = Color.Gray };
            zeroLCL = new Label() { Parent = parent, Left = 624, Top = 343, Text = "Zero", AutoSize = true };
            crcErrorLC = new Label() { Parent = parent, Left = 682, Top = 343, Width = 19, Height = 13, BackColor = Color.FromArgb(175, 0, 0) };
            crcErrorLCL = new Label() { Parent = parent, Left = 707, Top = 343, Text = "CRC Error", AutoSize = true };
            headerNotFoundLC = new Label() { Parent = parent, Left = 778, Top = 343, Width = 19, Height = 13, BackColor = Color.FromArgb(0, 0, 223) };
            headerNotFoundLCL = new Label() { Parent = parent, Left = 803, Top = 343, Text = "Header Not Found", AutoSize = true };
            processingLC = new Label() { Parent = parent, Left = 920, Top = 343, Width = 19, Height = 13, BackColor = Color.Black };
            processingLCL = new Label() { Parent = parent, Left = 945, Top = 343, Text = "Reading", AutoSize = true };
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
            sectorReadAttempts.TextChanged += SectorReadAttempts_TextChanged;
            trackFrom.TextChanged += SectorReadAttempts_TextChanged;
            trackTo.TextChanged += SectorReadAttempts_TextChanged;
            abortButton.Click += AbortButton;
            setSize.Click += SetSize;
            contextMenu.ResumeLayout(false);
            parent.ResumeLayout(false);
        }

        private void MarkSelectionAsGood_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Good, map.TrackFrom * SectorsOnTrack, (map.TrackTo - map.TrackFrom) * SectorsOnTrack);
            map.Repaint();
            stats.Repaint();
        }

        private void MarkAsGood_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Good, track * SectorsOnTrack + sector);
            map.Repaint();
            stats.Repaint();
        }

        private void MarkSelectionAsUnprocessed_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Unprocessed, map.TrackFrom * SectorsOnTrack, (map.TrackTo - map.TrackFrom) * SectorsOnTrack);
            map.Repaint();
            stats.Repaint();
        }

        private void MarkAsUnprocessed_Click(object sender, EventArgs e)
        {
            Image.SetSectorsProcessResult(SectorProcessResult.Unprocessed, track * SectorsOnTrack + sector);
            map.Repaint();
            stats.Repaint();
        }

        private void SectorReadAttempts_TextChanged(object sender, EventArgs e)
        {
            ReadParameters(false);
        }

        public void Abort()
        {
            if (diskReader != null) diskReader.Aborted = true;
        }

        private void SetSize(object sender, EventArgs e)
        {
            int sizeTracks = Image.SizeTracks;
            if (!InputBox.InputInt32("", "Введите размер в треках", ref sizeTracks, 1, MainForm.MaxTrack)) return;
            Image.SetSize(sizeTracks * SectorsOnTrack);
            map.Repaint();
            stats.Repaint();
            Log.Info?.Out($"Размер образа установлен: {sizeTracks} треков.");
        }

        protected virtual void RefreshControlsCustom()
        {

        }

        public void RefreshControls()
        {
            RefreshControlsCustom();
            if (Image != null)
            {
                map.Repaint();
                stats.Repaint();
            }
        }

        private void AbortButton(object sender, EventArgs e)
        {
            if (diskReader != null) diskReader.Aborted = true;
            abortButton.Enabled = false;
            aborted = true;
        }

        protected void SetEnabled()
        {
            readRandomSectors.Enabled = enabled && !processing && Image != null;
            readForward.Enabled = enabled && !processing && Image != null;
            readBackward.Enabled = enabled && !processing && Image != null;
            abortButton.Enabled = enabled && processing;
            loadImage.Enabled = enabled && !processing;
            setSize.Enabled = enabled && !processing && Image != null;
            mergeImage.Enabled = enabled && !processing && Image != null;
            newImage.Enabled = enabled && !processing;
            saveImage.Enabled = enabled && !processing && Image != null;
            readCatalogue.Enabled = enabled && !processing;
            showCatalogue.Enabled = enabled && Image != null;
            showCatFromTrack.Enabled = enabled && Image != null;
        }

        protected void SetProcessing(bool processing)
        {
            bool oldProcessing = this.processing;
            this.processing = processing;
            SetEnabled();
            if (processing)
            {
                if (!oldProcessing) OperationStarted?.Invoke(this, null);
            }
            else
            {
                if (oldProcessing) OperationCompleted?.Invoke(this, null);
            }
        }

        protected virtual bool ReadParametersCustom()
        {
            return true;
        }

        public bool ReadParameters(bool checkBoundaries = true)
        {
            int trackFrom;
            int trackTo;
            bool sraValid = Int32.TryParse(sectorReadAttempts.Text, out Params.SectorReadAttempts) && Params.SectorReadAttempts > 0;
            bool tfValid = Int32.TryParse(this.trackFrom.Text, out trackFrom) && trackFrom >= 0;
            bool tlValid = Int32.TryParse(this.trackTo.Text, out trackTo) && trackTo > trackFrom && trackTo <= MainForm.MaxTrack;
            sectorReadAttempts.BackColor = sraValid ? SystemColors.Window : Color.Red;
            this.trackFrom.BackColor = tfValid ? SystemColors.Window : Color.Red;
            this.trackTo.BackColor = tlValid ? SystemColors.Window : Color.Red;
            if (tfValid) map.TrackFrom = trackFrom;
            if (tlValid) map.TrackTo = trackTo;
            map.Repaint();
            Params.TrackFrom = trackFrom;
            Params.TrackTo = trackTo;
            Params.SectorNumFrom = Math.Min(trackFrom * SectorsOnTrack, Image.SizeSectors);
            Params.SectorNumTo = Math.Min(trackTo * SectorsOnTrack, Image.SizeSectors);
            if (checkBoundaries && Params.SectorNumFrom == Params.SectorNumTo)
            {
                Log.Info?.Out($"Область чтения вне пределов образа.");
                return false;
            }
            if (side0.Checked) Params.Side = DiskSide.Side0;
            if (side1.Checked) Params.Side = DiskSide.Side1;
            if (sideBoth.Checked) Params.Side = DiskSide.Both;
            int notGood = Image.GetNotGoodBounds(Params.SectorNumFrom, Params.SectorNumTo, Params.Side, ref Params.SectorNumFrom, ref Params.SectorNumTo);
            if (checkBoundaries && notGood == 0)
            {
                Log.Info?.Out($"Область чтения не содержит непрочитанных секторов.");
                return false;
            }
            Params.DataRate = dataRateArray[dataRate.SelectedIndex];
            Params.Image = Image;
            return sraValid & tfValid & tlValid & ReadParametersCustom();
        }

        private void ChartArea_MouseUp(object sender, MouseEventArgs e)
        {
            selecting = false;
            map.ClearHighlight();
            map.Repaint();
        }

        private void ChartArea_MouseDown(object sender, MouseEventArgs e)
        {
            map.GetTrackSectorByMousePosition(e.X, e.Y, out track, out sector);
            if (e.Button == MouseButtons.Right)
            {
                map.ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                int sectorNumber = track * map.SectorsOnTrack + sector;
                bool sectorSelected = sector >= 0 && sector < map.SectorsOnTrack && track >= 0 && track < MainForm.MaxTrack && sectorNumber < Image.SizeSectors;
                if (sectorSelected)
                {
                    map.WorkMap[sectorNumber] |= MapCell.Hover;
                    int diskSector = Params.ImageSectorLayout.Layout.Data[sector].SectorNumber;
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
                map.Repaint();
                mapMouseLeaveIgnore = true;
                contextMenu.Show(map.ChartArea, new Point(e.X, e.Y));
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            map.ClearHighlight();
            selecting = true;
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
            map.ClearHighlight(MapCell.Highlighted | MapCell.Hover);
            map.Repaint();
        }

        private void ChartArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (Image == null) return;
            int track;
            int sector;
            map.GetTrackSectorByMousePosition(e.X, e.Y, out track, out sector);
            int sectorNumber = track * map.SectorsOnTrack + sector;
            bool sectorSelected = sector >= 0 && sector < map.SectorsOnTrack && track >= 0 && track < MainForm.MaxTrack && sectorNumber < Image.SizeSectors;
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
                trackFrom.Text = minTrack.ToString();
                trackTo.Text = maxTrack.ToString();
                map.Select(minTrack, maxTrack);
            }
            trackLV.Text = track.ToString();
            if (sectorSelected)
            {
                int diskSector = Params.ImageSectorLayout.Layout.Data[sector].SectorNumber;
                sectorLV.Text = diskSector.ToString();
                statusLV.Text = Image.Sectors[sectorNumber].ToString();
                if (Image is TrDosImage)
                {
                    FileData file = ((TrDosImage)Image).GetFileByDiskAddress(track, sector);
                    if (file != null)
                    {
                        fileLV.Text = file.FileName;
                        extenstionLV.Text = file.Extension.ToString();
                    }
                    else
                    {
                        fileLV.Text = "";
                        extenstionLV.Text = "";
                    }
                    if (!selecting)
                    {
                        map.ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                        map.WorkMap[sectorNumber] |= MapCell.Hover;
                        if (file != null)
                        {
                            int sn = file.Track * map.SectorsOnTrack + file.Sector;
                            map.HighlightFile(sn, Math.Min(file.Size, Image.SizeSectors - sn));
                        }
                    }
                }
                else if (!selecting)
                {
                    map.ClearHighlight(MapCell.Highlighted | MapCell.Hover);
                    map.WorkMap[sectorNumber] |= MapCell.Hover;
                }
            }
            else
            {
                ClearTrackSectorFileName(false);
                map.ClearHighlight(MapCell.Hover);
            }
            map.Repaint();
        }

        private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ChartArea_MouseLeave(sender, e);
        }

        private void ClearTrackSectorFileName(bool clearTrack)
        {
            if (clearTrack) trackLV.Text = "";
            sectorLV.Text = "";
            statusLV.Text = "";
            fileLV.Text = "";
            extenstionLV.Text = "";
        }

        private void Map_HeaderDoubleClick(object sender, MouseEventArgs e)
        {
            trackFrom.Text = "0";
            trackTo.Text = MainForm.MaxTrack.ToString();
            map.TrackFrom = 0;
            map.TrackTo = MainForm.MaxTrack;
            map.Repaint();
        }
    }
}
