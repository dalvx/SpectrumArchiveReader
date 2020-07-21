using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class IsDosReader : ReaderBase
    {
        protected IsDosImage IsDosImage { get { return (IsDosImage)Image; } }

        public IsDosReader(Control parent, DataRate defaultDataRate) : base(parent, 1024, 5, defaultDataRate)
        {
            fileL.Visible = false;
            fileLV.Visible = false;
            extenstionLV.Visible = false;
            readCatalogue.Visible = false;
            showCatalogue.Visible = false;
            showCatFromTrack.Visible = false;
            stats.Headers[3] = "Размер FDI:";
            Image = new IsDosImage(160 * SectorsOnTrack, map) { Name = "" };
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            loadImage.Click += LoadImage;
            mergeImage.Click += MergeImage;
            newImage.Click += NewImage;
            saveImage.Click += SaveImage;
            readForward.Click += ReadForward;
            readBackward.Click += ReadBackward;
            readRandomSectors.Click += ReadRandomSectors;
            Params.ImageSectorLayout.SetFormat(TrackFormatName.IsDosSequential);
        }

        protected override bool ReadParametersCustom()
        {
            Params.UpperSideHead = UpperSideHead.Head1;
            Params.UpperSideHeadAutodetect = false;
            Params.ReadMode = ReadMode.Standard;
            Params.CurrentTrackFormat.SetFormat(TrackFormatName.IsDosSequential);
            return true;
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
            Image = new IsDosImage(size * SectorsOnTrack, map) { Name = value };
            stats.Image = Image;
            SetEnabled();
            Log.Info?.Out($"Образ диска создан. Имя: {value} | Размер: {size} треков ({size * SectorsOnTrack} секторов).");
        }

        private void SaveImage(object sender, EventArgs e)
        {
            if (Image == null) return;
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "FDI File (*.fdi)|*.fdi" };
            saveDialog.FileName = Image.Name;
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllBytes(saveDialog.FileName, IsDosImage.ToFdi(Params.ImageSectorLayout, null, 0));
            Image.ResetModify();
            Log.Info?.Out($"Образ сохранен. Имя: {Image.Name} | Секторов: {Image.FileSectorsSize} | Good: {Image.GoodSectors} | Bad: {Image.BadSectors} | FileName: {saveDialog.FileName}");
        }

        private void LoadImage(object sender, EventArgs e)
        {
            if (Image != null && Image.Modified)
            {
                if (MessageBox.Show("Образ не был сохранен. Продолжить?", "", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            }
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = "FDI Files (*.fdi)|*.fdi|All Files (*.*)|*.*" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            IsDosImage image = new IsDosImage();
            string text;
            if (image.LoadFdi(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), new TrackFormat(TrackFormatName.IsDosSequential), out text, map) != 0)
            {
                Log.Error?.Out($"Ошибка при чтении файла: {openDialog.FileName}");
                return;
            }
            Image = image;
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            SetEnabled();
            int loadedSize = Image.SizeTracks;
            Log.Info?.Out($"Образ загружен. Имя: {Image.Name} | Размер: {loadedSize} треков | FileName: {openDialog.FileName}");
        }

        private void MergeImage(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = "FDI Files (*.fdi)|*.fdi|All Files (*.*)|*.*" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            IsDosImage image = new IsDosImage();
            string text;
            if (image.LoadFdi(openDialog.FileName, File.ReadAllBytes(openDialog.FileName), new TrackFormat(TrackFormatName.IsDosSequential), out text) != 0)
            {
                Log.Error?.Out($"Ошибка при чтении файла: {openDialog.FileName}");
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
            Log.Info?.Out($"Чтение диска. Side: {Params.Side}. DataRate: {Params.DataRate}.");
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
            Log.Info?.Out($"Чтение диска в обратном направлении. Side: {Params.Side}. DataRate: {Params.DataRate}.");
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
            Log.Info?.Out($"Чтение случайных секторов. Side: {Params.Side}. DataRate: {Params.DataRate}");
            diskReader = new DiskReader((DiskReaderParams)Params.Clone());
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    diskReader.OpenDriver();
                    int successfullyRead = diskReader.ReadRandomSectors(TimeSpan.Zero);
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
