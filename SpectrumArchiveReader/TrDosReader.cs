using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class TrDosReader : ReaderBase
    {
        protected Label trackLayoutL;
        protected Label trackLayoutLV;
        protected GroupBox upperSidePanel;
        protected RadioButton upperSide0;
        protected RadioButton upperSide1;
        protected RadioButton upperSideAutodetect;
        protected GroupBox readModePanel;
        protected RadioButton readModeStandard;
        protected RadioButton readModeFast;
        private int showCatFromTrackNumber = 162;
        protected int loadImageSize = 0;
        private TrDosImage TrDosImage { get { return (TrDosImage)Image; } }
        private string upperSideString;
        private TrackFormatName trackLayout;

        public TrDosReader(Control parent, DataRate defaultDataRate) : base(parent, 256, 16, defaultDataRate)
        {
            filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*";
            upperSidePanel = new GroupBox() { Parent = parent, Left = 316, Top = 99, Width = 156, Height = 89, Text = "Upper Side Head Parameter" };
            upperSide0 = new RadioButton() { Parent = upperSidePanel, Left = 6, Top = 20, Text = "Head = 0", AutoSize = true };
            upperSide1 = new RadioButton() { Parent = upperSidePanel, Left = 6, Top = 43, Text = "Head = 1", AutoSize = true };
            upperSideAutodetect = new RadioButton() { Parent = upperSidePanel, Left = 6, Top = 66, Text = "Autodetect", AutoSize = true, Checked = true };
            readModePanel = new GroupBox() { Parent = parent, Left = 202, Top = 99, Width = 105, Height = 62, Text = "Read Mode" };
            readModeStandard = new RadioButton() { Parent = readModePanel, Left = 6, Top = 19, Text = "Standard", AutoSize = true, Checked = !Stopwatch.IsHighResolution };
            readModeFast = new RadioButton() { Parent = readModePanel, Left = 6, Top = 39, Text = "Fast", AutoSize = true, Checked = Stopwatch.IsHighResolution, Enabled = Stopwatch.IsHighResolution };
            trackLayoutL = new Label() { Parent = parent, Left = 0, Top = 205, Text = "Sector Layout:", AutoSize = true };
            trackLayoutLV = new Label() { Parent = parent, Left = 75, Top = 205, Text = "...", AutoSize = true };
            trackLayoutL.Visible = MainForm.Dev;
            trackLayoutLV.Visible = MainForm.Dev;
            Image = new TrDosImage(160 * SectorsOnTrack, map) { Name = "" };
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            saveImage.Click += SaveImage;
            readCatalogue.Click += ReadCatalogue;
            showCatalogue.Click += ShowCatalogue;
            showCatFromTrack.Click += ShowCatFromTrack;
            loadImage.Click += LoadImage;
            mergeImage.Click += MergeImage;
            newImage.Click += NewImage;
            readForward.Click += ReadForward;
            readBackward.Click += ReadBackward;
            readRandomSectors.Click += ReadRandomSectors;
            Params.ImageSectorLayout.SetFormat(TrackFormatName.TrDosSequential);
        }

        private void ShowCatFromTrack(object sender, EventArgs e)
        {
            int track = showCatFromTrackNumber;
            if (!InputBox.InputInt32("", "Введите номер трека", ref track, 0, Image.SizeTracks - 1)) return;
            showCatFromTrackNumber = track;
            if (MainForm.CatalogueForm == null) MainForm.CatalogueForm = new CatalogueForm();
            MainForm.CatalogueForm.ShowForm(TrDosImage, true, track * SectorsOnTrack);
        }

        private void ShowCatalogue(object sender, EventArgs e)
        {
            if (MainForm.CatalogueForm == null) MainForm.CatalogueForm = new CatalogueForm();
            MainForm.CatalogueForm.ShowForm(TrDosImage, true);
        }

        private void SaveImage(object sender, EventArgs e)
        {
            if (Image == null) return;
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = filter };
            saveDialog.FileName = Image.Name;
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllBytes(saveDialog.FileName, TrDosImage.ToTrd(32));
            Image.ResetModify();
            Log.Info?.Out($"Образ сохранен. Имя: {Image.Name} | Секторов: {Image.FileSectorsSize} | Good: {Image.GoodSectors} | Bad: {Image.BadSectors} | FileName: {saveDialog.FileName}");
        }

        protected override bool ReadParametersCustom()
        {
            Params.UpperSideHead = upperSide0.Checked ? UpperSideHead.Head0 : UpperSideHead.Head1;
            Params.UpperSideHeadAutodetect = upperSideAutodetect.Checked;
            if (Params.UpperSideHeadAutodetect)
            {
                upperSideString = "Autodetect";
            }
            else
            {
                upperSideString = Params.UpperSideHead == UpperSideHead.Head0 ? "Head = 0" : "Head = 1";
            }
            Params.CurrentTrackFormat.SetFormat(TrackFormatName.TrDosTurbo);
            Params.ReadMode = readModeStandard.Checked ? ReadMode.Standard : ReadMode.Fast;
            Params.TrackLayoutAutodetect = Params.ReadMode == ReadMode.Fast;
            trackLayout = TrackFormatName.NoHeaders;
            return true;
        }

        protected override void RefreshControlsCustom()
        {
            if (diskReader?.Params?.ReadMode == ReadMode.Fast && trackLayout != diskReader.Params.CurrentTrackFormat?.FormatName)
            {
                trackLayout = diskReader.Params.CurrentTrackFormat.FormatName;
                trackLayoutLV.Text = trackLayout.ToString().Replace("TrDos", "");
            }
        }

        private void ReadCatalogue(object sender, EventArgs e)
        {
            if (!ReadParameters(false)) return;
            SetProcessing(true);
            BackgroundWorker worker = new BackgroundWorker();
            Log.Info?.Out($"Чтение каталога. DataRate: {Params.DataRate}");
            int successfullyRead = 0;
            DiskReaderParams npars = new DiskReaderParams()
            {
                SectorNumFrom = 0,
                SectorNumTo = 9,
                DataRate = Params.DataRate,
                Image = new TrDosImage(9, null),
                SectorReadAttempts = Params.SectorReadAttempts,
                SectorSize = Params.SectorSize,
                SectorsOnTrack = Params.SectorsOnTrack,
                Side = DiskSide.Both,
                CurrentTrackFormat = new TrackFormat(TrackFormatName.TrDosSequential),
                ImageSectorLayout = new TrackFormat(TrackFormatName.TrDosSequential),
                ReadMode = ReadMode.Standard,
                TrackLayoutAutodetect = false,
                UpperSideHeadAutodetect = false,
            };
            diskReader = new DiskReader() { Params = npars };
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    diskReader.OpenDriver();
                    successfullyRead = diskReader.ReadForward();
                    diskReader.CloseDriver();
                    Log.Info?.Out($"Чтение каталога завершено. Успешно прочитанных секторов: {successfullyRead}");
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение при чтении диска: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                SetProcessing(false);
                if (MainForm.CatalogueForm == null) MainForm.CatalogueForm = new CatalogueForm();
                if (successfullyRead > 1) MainForm.CatalogueForm.ShowForm(diskReader.Params.Image as TrDosImage, false);
            };
            worker.RunWorkerAsync();
        }

        private void NewImage(object sender, EventArgs e)
        {
            if (Image != null && Image.Modified)
            {
                if (MessageBox.Show("Образ не был сохранен. Продолжить?", "", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            }
            string value = newImageName;
            if (!InputBox.Query("", "Введите имя образа", ref value)) return;
            int size = newImageSize;
            if (!InputBox.InputInt32("", "Введите размер в треках", ref size, 1, MainForm.MaxTrack)) return;
            newImageName = value;
            newImageSize = size;
            Image = new TrDosImage(size * SectorsOnTrack, map) { Name = value };
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            SetEnabled();
            Log.Info?.Out($"Образ диска создан. Имя: {value} | Размер: {size} треков ({size * SectorsOnTrack} секторов).");
        }

        private void LoadImage(object sender, EventArgs e)
        {
            if (Image != null && Image.Modified)
            {
                if (MessageBox.Show("Образ не был сохранен. Продолжить?", "", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            }
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = filter };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            int size = loadImageSize;
            if (!InputBox.InputInt32("", "Введите количество треков образа\n(0 - автоопределение)", ref size, 0, MainForm.MaxTrack)) return;
            loadImageSize = size;
            Image = new TrDosImage();
            Image.Load(openDialog.FileName, size * SectorsOnTrack, map);
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            SetEnabled();
            int loadedSize = Image.SizeTracks;
            Log.Info?.Out($"Образ загружен. Имя: {Image.Name} | Размер: {loadedSize} треков {(size == 0 ? "(автоопределение)" : "")} | FileName: {openDialog.FileName}");
        }

        private void MergeImage(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = filter };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            TrDosImage image = new TrDosImage();
            image.Load(openDialog.FileName);
            int addedReadSectors;
            Image.Merge(image, out addedReadSectors);
            map.Repaint();
            stats.Repaint();
            Log.Info?.Out($"Образ слит с образом из файла: {openDialog.FileName}. Добавлено прочитанных секторов: {addedReadSectors}");
        }

        private void ReadForward(object sender, EventArgs e)
        {
            if (!ReadParameters()) return;
            SetProcessing(true);
            Log.Info?.Out($"Чтение диска. UpperSideHead: {upperSideString}. Side: {Params.Side}. DataRate: {Params.DataRate}. ReadMode: {(Params.TrackLayoutAutodetect ? "Fast" : "Standard")}");
            diskReader = new DiskReader((DiskReaderParams)Params.Clone());
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    diskReader.OpenDriver();
                    int successfullyRead = diskReader.ReadForward();
                    diskReader.CloseDriver();
                    Log.Info?.Out($"Успешно прочитанных секторов: {successfullyRead}");
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение при чтении диска: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                SetProcessing(false);
            };
            worker.RunWorkerAsync();
        }

        private void ReadBackward(object sender, EventArgs e)
        {
            if (!ReadParameters()) return;
            SetProcessing(true);
            Log.Info?.Out($"Чтение диска в обратном направлении. UpperSideHead: {upperSideString}. Side: {Params.Side}. DataRate: {Params.DataRate}. ReadMode: {(Params.TrackLayoutAutodetect ? "Fast" : "Standard")}");
            diskReader = new DiskReader((DiskReaderParams)Params.Clone());
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    diskReader.OpenDriver();
                    int successfullyRead = diskReader.ReadBackward();
                    diskReader.CloseDriver();
                    Log.Info?.Out($"Успешно прочитанных секторов: {successfullyRead}");
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение при чтении диска: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                SetProcessing(false);
            };
            worker.RunWorkerAsync();
        }

        private void ReadRandomSectors(object sender, EventArgs e)
        {
            if (!ReadParameters()) return;
            SetProcessing(true);
            Log.Info?.Out($"Чтение случайных секторов. UpperSideHead: {upperSideString}. Side: {Params.Side}. DataRate: {Params.DataRate}");
            diskReader = new DiskReader((DiskReaderParams)Params.Clone());
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    diskReader.OpenDriver();
                    int successfullyRead = diskReader.ReadRandomSectors(TimeSpan.Zero, 10);
                    diskReader.CloseDriver();
                    Log.Info?.Out($"Успешно прочитанных секторов: {successfullyRead}");
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение при чтении диска: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                SetProcessing(false);
            };
            worker.RunWorkerAsync();
        }
    }
}
