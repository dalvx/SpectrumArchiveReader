using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            Help_Resize(this, null);
        }

        private void Help_Resize(object sender, EventArgs e)
        {
            richTextBox1.Width = ClientSize.Width;
            richTextBox1.Height = ClientSize.Height - C_Close.Height - 30;
            C_Close.Top = ClientSize.Height - C_Close.Height - 15;
            C_Close.Left = (ClientSize.Width - C_Close.Width) - 15;
        }

        private void C_Close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
