using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class CpmReader : ReaderBase
    {
        protected CpmImage CpmImage { get { return (CpmImage)Image; } }

        public CpmReader(Control parent, DiskReaderParams dparams) : base(parent, 1024, 5, dparams)
        {
            upperSidePanel.Visible = false;
            readModePanel.Visible = false;
            map.ZeroLCL.Text = "Empty";
            map.ZeroByte = 0xE5;
            map.FileL.Visible = false;
            map.FileLV.Visible = false;
            map.ExtenstionLV.Visible = false;
            readCatalogue.Visible = false;
            showCatalogue.Visible = false;
            showCatFromTrack.Visible = false;
            Image = new CpmImage(160 * SectorsOnTrack, map) { Name = "" };
            map.Image = Image;
            stats.Image = Image;
            map.Repaint();
            stats.Repaint();
            loadImage.Visible = false;
            mergeImage.Visible = false;
            newImage.Click += NewImage;
            saveImage.Click += SaveImage;
            readForward.Click += ReadForward;
            readBackward.Click += ReadBackward;
            readRandomSectors.Click += ReadRandomSectors;
        }

        protected override bool ReadParametersCustom()
        {
            Params.UpperSideHead = UpperSideHead.Head1;
            Params.UpperSideHeadAutodetect = false;
            Params.ReadMode = ReadMode.Standard;
            Params.CurrentTrackFormat.SetFormat(TrackFormatName.CpmSequential);
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
            Image = new CpmImage(size * SectorsOnTrack, map) { Name = value };
            map.Image = Image;
            stats.Image = Image;
            SetEnabled();
            Log.Info?.Out($"Образ диска создан. Имя: {value} | Размер: {size} треков ({size * SectorsOnTrack} секторов).");
        }

        private void SaveImage(object sender, EventArgs e)
        {
            if (Image == null) return;
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "KDI File (*.kdi)|*.kdi|FDI File (*.fdi)|*.fdi" };
            saveDialog.FileName = Image.Name;
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            if (saveDialog.FilterIndex == 1)
            {
                File.WriteAllBytes(saveDialog.FileName, CpmImage.ToKdi(0));
            }
            else
            {
                File.WriteAllBytes(saveDialog.FileName, CpmImage.ToFdi(null, 0));
            }
            Image.ResetModify();
            Log.Info?.Out($"Образ сохранен. Имя: {Image.Name} | Секторов: {Image.FileSectorsSize} | Good: {Image.GoodSectors} | Bad: {Image.BadSectors} | FileName: {saveDialog.FileName}");
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
