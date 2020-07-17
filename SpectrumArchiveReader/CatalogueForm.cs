using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public partial class CatalogueForm : Form
    {
        private TrDosImage image;

        public CatalogueForm()
        {
            InitializeComponent();
            Catalogue_Resize(this, null);
        }

        private void Catalogue_Resize(object sender, EventArgs e)
        {
            richTextBox1.Width = ClientSize.Width;
            richTextBox1.Height = ClientSize.Height - C_Close.Height - 30;
            C_Close.Top = ClientSize.Height - C_Close.Height - 15;
            C_Close.Left = (ClientSize.Width - C_Close.Width) - 15;
            C_SaveAsTxt.Top = C_Close.Top;
            C_SaveAsTxt.Left = 15;
        }

        private void C_Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void ShowForm(TrDosImage image, bool showBad, int sectorNumber = 0)
        {
            this.image = image;
            image.ParseCatalogue(sectorNumber);
            Text = image.Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MakeStr($"Title:   {image.Title}", 20));
            sb.AppendLine(MakeStr($"Files:   {image.FileCount8Sector}", 20));
            sb.AppendLine(MakeStr($"Deleted: {image.DeletedFiles8Sector}", 20));
            sb.AppendLine(MakeStr($"Free:    {image.Free8Sector}", 20));
            sb.AppendLine(MakeStr($"Type:    {image.DiskType}", 20));
            string h = "\n" + new string('-', showBad ? 74 : 49);
            sb.AppendLine(h);
            sb.Append(MakeStr("Filename", 9) + MakeStr("Ext", 5) + MakeStr("Size", 5) + MakeStr("Start", 8) + MakeStr("Length", 8) + MakeStr("Track", 7)
                + MakeStr("Sector", 7));
            if (showBad) sb.Append(MakeStr("Good", 7) + MakeStr("Bad", 5) + MakeStr("Unprocessed", 13));
            sb.AppendLine(h);
            for (int i = 0; i < image.Files.Cnt; i++)
            {
                FileData file = image.Files[i];
                sb.Append(MakeStr(file.FileName, 9) + MakeStr(file.Extension.ToString(), 5) + MakeStr(file.Size, 5) + MakeStr(file.Start, 8) +
                    MakeStr(file.Length, 8) + MakeStr(file.Track, 7) + MakeStr(file.Sector, 7));
                if (showBad) sb.Append(MakeStr(file.GoodSectors, 7) + MakeStr(file.BadSectors, 5) + MakeStr(file.UnprocessedSectors, 13));
                sb.AppendLine();
            }
            richTextBox1.Text = sb.ToString();
            image.ParseCatalogue();
            ShowDialog();
        }

        private string MakeStr(string s, int len)
        {
            if (len < s.Length) len = s.Length;
            StringBuilder sb = new StringBuilder(s, len);
            sb.Append(' ', len - s.Length);
            return sb.ToString();
        }

        private string MakeStr(int i, int len)
        {
            return MakeStr(i.ToString(), len);
        }

        private void C_SaveAsTxt_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                FileName = image.Name,
                Filter = "txt Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < image.Files.Cnt; i++)
            {
                FileData file = image.Files[i];
                sb.AppendLine(MakeStr(file.FileName, 9) + file.Extension.ToString());
            }
            File.WriteAllText(saveDialog.FileName, sb.ToString());
        }
    }
}
