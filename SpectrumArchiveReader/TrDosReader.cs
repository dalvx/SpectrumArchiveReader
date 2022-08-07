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
        private int showCatFromTrackNumber = 162;
        private TrDosImage TrDosImage { get { return (TrDosImage)Image; } }
        private string upperSideString;
        private TrackFormatName trackLayout;

        public TrDosReader(Control parent, DiskReaderParams dparams) : base(parent, 256, 16, dparams)
        {
            Image = new TrDosImage(160 * SectorsOnTrack, map) { Name = "" };
            map.Image = Image;
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
                FirstSectorNum = 0,
                LastSectorNum = 9,
                DataRate = Params.DataRate,
                Image = new TrDosImage(9, null),
                SectorReadAttempts = Params.SectorReadAttempts,
                Side = DiskSide.Both,
                CurrentTrackFormat = new TrackFormat(TrackFormatName.TrDosSequential),
                ReadMode = ReadMode.Standard,
                TrackLayoutAutodetect = false,
                UpperSideHeadAutodetect = false,
                Drive = Params.Drive
            };
            diskReader = new DiskReader() { Params = npars };
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    if (!diskReader.OpenDriver()) return;
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
            map.Image = Image;
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            SetEnabled();
            Log.Info?.Out($"Образ диска создан. Имя: {value} | Размер: {size} треков ({size * SectorsOnTrack} секторов).");
        }

        private void SaveImage(object sender, EventArgs e)
        {
            if (Image == null) return;
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "FDI File (*.fdi)|*.fdi|TRD File (*.trd)|*.trd|Modified TRD (*.trd)|*.trd" };
            saveDialog.FileName = Image.Name;
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            if (saveDialog.FilterIndex == 1)
            {
                File.WriteAllBytes(saveDialog.FileName, TrDosImage.ToFdi(null, 32));
            }
            else if (saveDialog.FilterIndex == 2)
            {
                File.WriteAllBytes(saveDialog.FileName, TrDosImage.ToTrd(32, false));
            }
            else
            {
                File.WriteAllBytes(saveDialog.FileName, TrDosImage.ToTrd(32, true));
            }
            Image.ResetModify();
            Log.Info?.Out($"Образ сохранен. Имя: {Image.Name} | Секторов: {Image.FileSectorsSize} | Good: {Image.GoodSectors} | Bad: {Image.NotGoodSectors} | FileName: {saveDialog.FileName}");
        }

        private void LoadImage(object sender, EventArgs e)
        {
            if (Image != null && Image.Modified)
            {
                if (MessageBox.Show("Образ не был сохранен. Продолжить?", "", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            }
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = "TRD (*.trd)|*.trd|Modified TRD (*.trd)|*.trd|FDI (*.fdi)|*.fdi|All Files (*.*)|*.*" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            TrDosImage image = new TrDosImage();
            int result;
            if (openDialog.FilterIndex == 1)
            {
                result = image.LoadTrd(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), false, map);
            }
            else if (openDialog.FilterIndex == 2)
            {
                result = image.LoadTrd(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), true, map);
            }
            else if (openDialog.FilterIndex == 3)
            {
                string text;
                result = image.LoadFdi(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), out text, map);
            }
            else
            {
                result = image.LoadAutodetect(openDialog.FileName, map);
            }
            if (result != 0)
            {
                Log.Error?.Out($"Ошибка при чтении файла: {openDialog.FileName}");
                return;
            }
            Image = image;
            map.Image = Image;
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            SetEnabled();
            int loadedSize = Image.SizeTracks;
            Log.Info?.Out($"Образ загружен. Имя: {Image.Name} | Размер: {loadedSize} треков | FileName: {openDialog.FileName}");
        }

        private void MergeImage(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = "TRD (*.trd)|*.trd|Modified TRD (*.trd)|*.trd|FDI (*.fdi)|*.fdi|All Files (*.*)|*.*" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            TrDosImage image = new TrDosImage();
            int result;
            if (openDialog.FilterIndex == 1)
            {
                result = image.LoadTrd(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), false);
            }
            else if (openDialog.FilterIndex == 2)
            {
                result = image.LoadTrd(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), true);
            }
            else if (openDialog.FilterIndex == 3)
            {
                string text;
                result = image.LoadFdi(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), out text);
            }
            else
            {
                result = image.LoadAutodetect(openDialog.FileName);
            }
            if (result != 0)
            {
                Log.Warn?.Out($"Ошибка при чтении файла: {openDialog.FileName}");
                return;
            }
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
                    if (!diskReader.OpenDriver()) return;
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
                    if (!diskReader.OpenDriver()) return;
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
                    if (!diskReader.OpenDriver()) return;
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
