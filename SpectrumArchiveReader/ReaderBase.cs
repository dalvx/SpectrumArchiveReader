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
        protected Label dataRateL;
        protected ComboBox dataRate;
        protected Label trackLayoutL;
        protected Label trackLayoutLV;
        protected GroupBox upperSidePanel;
        protected RadioButton upperSide0;
        protected RadioButton upperSide1;
        protected RadioButton upperSideAutodetect;
        protected GroupBox readModePanel;
        protected RadioButton readModeStandard;
        protected RadioButton readModeFast;
        protected bool aborted;
        protected Map map;
        protected ImageStatsTable stats;
        public DiskImage Image;
        protected DiskReader diskReader;
        protected int newImageSize = 160;
        public DiskReaderParams Params;
        public static DataRate[] DataRateArray = new DataRate[] { DataRate.FD_RATE_250K, DataRate.FD_RATE_300K, DataRate.FD_RATE_500K, DataRate.FD_RATE_1M };
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

        public ReaderBase(Control parent, int sectorSize, int sectorsOnTrack, DiskReaderParams dparams)
        {
            Parent = parent;
            SectorSize = sectorSize;
            SectorsOnTrack = sectorsOnTrack;
            parent.SuspendLayout();
            Params = dparams;
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
            showCatalogue = new Button() { Parent = imageGB, Left = 170, Top = 184, Width = 75, Height = 23, Text = "Catalogue" };
            showCatFromTrack = new Button() { Parent = imageGB, Left = 250, Top = 184, Width = 128, Height = 23, Text = "Catalogue From Track" };
            setSize = new Button() { Parent = imageGB, Left = 10, Top = 184, Width = 75, Height = 23, Text = "Set Size" };
            mergeImage = new Button() { Parent = imageGB, Left = 90, Top = 184, Width = 75, Height = 23, Text = "Merge" };
            readForward = new Button() { Parent = parent, Left = 549, Top = 10, Width = 96, Height = 23, Text = "Read Forward" };
            readBackward = new Button() { Parent = parent, Left = 549, Top = 36, Width = 96, Height = 23, Text = "Read Backward" };
            readRandomSectors = new Button() { Parent = parent, Left = 511, Top = 72, Width = 134, Height = 23, Text = "Read Random Sectors" };
            abortButton = new Button() { Parent = parent, Left = 568, Top = 193, Width = 75, Height = 23, Text = "Abort" };
            readCatalogue = new Button() { Parent = parent, Left = 6, Top = 50, Width = 144, Height = 23, Text = "Read Catalogue" };
            dataRateL = new Label() { Parent = parent, Left = 3, Top = 10, Text = "Data Rate", AutoSize = true };
            dataRate = new ComboBox() { Parent = parent, Left = 6, Top = 26, Width = 121, Height = 21, DropDownStyle = ComboBoxStyle.DropDownList };
            upperSidePanel = new GroupBox() { Parent = parent, Left = 316, Top = 99, Width = 156, Height = 89, Text = "Upper Side Head Parameter" };
            upperSide0 = new RadioButton() { Parent = upperSidePanel, Left = 6, Top = 20, Text = "Head = 0", AutoSize = true };
            upperSide1 = new RadioButton() { Parent = upperSidePanel, Left = 6, Top = 43, Text = "Head = 1", AutoSize = true };
            upperSideAutodetect = new RadioButton() { Parent = upperSidePanel, Left = 6, Top = 66, Text = "Autodetect", AutoSize = true, Checked = true };
            readModePanel = new GroupBox() { Parent = parent, Left = 202, Top = 99, Width = 105, Height = 62, Text = "Read Mode" };
            readModeStandard = new RadioButton() { Parent = readModePanel, Left = 6, Top = 19, Text = "Standard", AutoSize = true, Checked = !Timer.IsHighResolution };
            readModeFast = new RadioButton() { Parent = readModePanel, Left = 6, Top = 39, Text = "Fast", AutoSize = true, Checked = Timer.IsHighResolution, Enabled = Timer.IsHighResolution };
            trackLayoutL = new Label() { Parent = parent, Left = 0, Top = 205, Text = "Sector Layout:", AutoSize = true };
            trackLayoutLV = new Label() { Parent = parent, Left = 75, Top = 205, Text = "...", AutoSize = true };
            trackLayoutL.Visible = MainForm.Dev;
            trackLayoutLV.Visible = MainForm.Dev;
            for (int i = 0; i < DataRateArray.Length; i++)
            {
                dataRate.Items.Add(DataRateArray[i].ToString().Replace("FD_RATE_" , ""));
            }
            FillControls();
            stats = new ImageStatsTable(imageGB, SystemColors.Window, SystemColors.ControlText);
            stats.SetPosition(6, 16);
            stats.Repaint();
            map = new Map(MainForm.MaxTrack, SectorSize, SectorsOnTrack, parent, Color.White, stats);
            map.SetPosition(0, 227);
            map.ReadBoundsChanged += Map_ReadBoundsChanged;
            map.TrackFrom = Params.TrackFrom;
            map.TrackTo = Params.TrackTo;
            map.Repaint();

            sectorReadAttempts.TextChanged += SectorReadAttempts_TextChanged;
            trackFrom.TextChanged += SectorReadAttempts_TextChanged;
            trackTo.TextChanged += SectorReadAttempts_TextChanged;
            side0.CheckedChanged += SectorReadAttempts_TextChanged;
            side1.CheckedChanged += SectorReadAttempts_TextChanged;
            sideBoth.CheckedChanged += SectorReadAttempts_TextChanged;
            dataRate.SelectedIndexChanged += SectorReadAttempts_TextChanged;
            upperSide0.CheckedChanged += SectorReadAttempts_TextChanged;
            upperSide1.CheckedChanged += SectorReadAttempts_TextChanged;
            readModeStandard.CheckedChanged += SectorReadAttempts_TextChanged;
            readModeFast.CheckedChanged += SectorReadAttempts_TextChanged;
            abortButton.Click += AbortButton;
            setSize.Click += SetSize;
            parent.ResumeLayout(false);
        }

        private void FillControls()
        {
            side0.Checked = Params.Side == DiskSide.Side0;
            side1.Checked = Params.Side == DiskSide.Side1;
            sideBoth.Checked = Params.Side == DiskSide.Both;
            sectorReadAttempts.Text = Params.SectorReadAttempts.ToString();
            trackFrom.Text = Params.TrackFrom.ToString();
            trackTo.Text = Params.TrackTo.ToString();
            for (int i = 0; i < DataRateArray.Length; i++)
            {
                if (DataRateArray[i] == Params.DataRate)
                {
                    dataRate.SelectedIndex = i;
                    break;
                }
            }
            readModeStandard.Checked = Params.ReadMode == ReadMode.Standard;
            readModeFast.Checked = Params.ReadMode == ReadMode.Fast;
            upperSide0.Checked = Params.UpperSideHead == UpperSideHead.Head0;
            upperSide1.Checked = Params.UpperSideHead == UpperSideHead.Head1;
            upperSideAutodetect.Checked = Params.UpperSideHeadAutodetect;
        }

        private void Map_ReadBoundsChanged(object sender, EventArgs e)
        {
            trackFrom.Text = map.TrackFrom.ToString();
            trackTo.Text = map.TrackTo.ToString();
        }

        protected void SectorReadAttempts_TextChanged(object sender, EventArgs e)
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
            Params.DataRate = DataRateArray[dataRate.SelectedIndex];
            Params.Image = Image;
            return sraValid & tfValid & tlValid & ReadParametersCustom();
        }
    }
}
