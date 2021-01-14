using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public partial class SectorContentsForm : Form
    {
        private DiskImage2 image;
        private int track;
        private int sectorIndex;

        public SectorContentsForm()
        {
            InitializeComponent();
            SectorContents_Resize(this, null);
        }

        private void SectorContents_Resize(object sender, EventArgs e)
        {
            richTextBox1.Width = ClientSize.Width - 30;
            richTextBox1.Height = ClientSize.Height - C_Close.Height - 50;
            C_Close.Top = ClientSize.Height - C_Close.Height - 15;
            C_Close.Left = (ClientSize.Width - C_Close.Width) - 15;
            C_PrevSector.Top = C_Close.Top;
            C_PrevSector.Left = 20;
            C_NextSector.Top = C_Close.Top;
            C_NextSector.Left = C_PrevSector.Right + 20;
        }

        public void Show(DiskImage2 image, int track, int sectorIndex)
        {
            this.image = image;
            this.track = track;
            this.sectorIndex = sectorIndex;
            ShowSector();
            ShowDialog();
        }

        private void ShowSector()
        {
            richTextBox1.Clear();
            SectorInfo sector = image[track][sectorIndex];
            TrackLV.Text = track.ToString();
            SectorLV.Text = sector.SectorNumber.ToString();
            SizeLV.Text = sector.SizeBytes.ToString();
            int strs = sector.SizeBytes / 16;
            byte[] temp = new byte[16];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strs; i++)
            {
                sb.Length = 0;
                sb.Append(BitConverter.ToString(sector.Data, i * 16, 16));
                sb.Append("  ");
                for (int t = 0; t < 16; t++)
                {
                    temp[t] = sector.Data[i * 16 + t] < 32 ? (byte)'.' : sector.Data[i * 16 + t];
                }
                sb.AppendLine(Encoding.ASCII.GetString(temp, 0, 16));
                richTextBox1.AppendText(sb.ToString());
            }
        }

        private void PrevSector_Click(object sender, EventArgs e)
        {
            int t = track;
            int si = sectorIndex;
            si--;
            while (si < 0 || (image[t][si].ProcessResult != SectorProcessResult.Good && image[t][si].ProcessResult != SectorProcessResult.Bad))
            {
                t--;
                if (t < 0) return;
                si = image[t].Layout.Cnt - 1;
            }
            track = t;
            sectorIndex = si;
            ShowSector();
        }

        private void NextSector_Click(object sender, EventArgs e)
        {
            int t = track;
            int si = sectorIndex;
            si++;
            while (si >= image[t].Layout.Cnt || (image[t][si].ProcessResult != SectorProcessResult.Good && image[t][si].ProcessResult != SectorProcessResult.Bad))
            {
                t++;
                if (t >= image.Tracks.Cnt) return;
                si = 0;
            }
            track = t;
            sectorIndex = si;
            ShowSector();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
