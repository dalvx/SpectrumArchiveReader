using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SpectrumArchiveReader
{
    public class InputBox : Form
    {
        private Label label;
        private TextBox textBox;
        private Button buttonOK;
        private Button buttonCancel;

        private InputBox(string caption, string text)
        {
            label = new Label();
            textBox = new TextBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            label.AutoSize = true;
            label.Location = new Point(9, 13);
            label.Name = "label";
            label.TabIndex = 1;
            label.Text = text;
            Controls.Add(label);
            textBox.Top = label.Bottom + 5;
            textBox.Left = 30;
            int clientWidth = Math.Max(300, label.Width + 13 + 13);
            int texBoxWidth = clientWidth - textBox.Left * 2;
            textBox.Name = "textValue";
            textBox.Size = new Size(texBoxWidth, 20);
            textBox.TabIndex = 2;
            textBox.WordWrap = false;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            int lenBetweenButtons = 30;
            int buttonOkLeft = (clientWidth - (buttonOK.Width + buttonCancel.Width + lenBetweenButtons)) / 2;
            buttonOK.Location = new Point(buttonOkLeft, textBox.Bottom + 15);
            buttonCancel.Location = new Point(buttonOK.Right + lenBetweenButtons, buttonOK.Top);
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(clientWidth, buttonCancel.Bottom + 15);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputBox";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = caption;
            ResumeLayout(false);
            PerformLayout();
        }

        public static bool Query(string caption, string text, ref string strValue)
        {
            InputBox ib = new InputBox(caption, text);
            ib.textBox.Text = strValue;
            if (ib.ShowDialog() != DialogResult.OK) return false;
            strValue = ib.textBox.Text;
            return true;
        }

        public static bool InputValue(string Caption, string Text, string prefix, string format, ref int value, int min, int max)
        {
            int val = value;

            string s_val = prefix + value.ToString(format);
            bool OKVal;
            do
            {
                OKVal = true;
                if (!Query(Caption, Text, ref s_val)) return false;

                try
                {
                    string sTr = s_val.Trim();

                    if ((sTr.Length > 0) && (sTr[0] == '#'))
                    {
                        sTr = sTr.Remove(0, 1);
                        val = Convert.ToInt32(sTr, 16);
                    }
                    else if ((sTr.Length > 1) && ((sTr[1] == 'x') && (sTr[0] == '0')))
                    {
                        sTr = sTr.Remove(0, 2);
                        val = Convert.ToInt32(sTr, 16);
                    }
                    else
                        val = Convert.ToInt32(sTr, 10);
                }
                catch { MessageBox.Show("Требуется ввести число!"); OKVal = false; }
                if ((val < min) || (val > max)) { MessageBox.Show("Требуется число в диапазоне " + min.ToString() + "..." + max.ToString() + " !"); OKVal = false; }
            } while (!OKVal);
            value = val;
            return true;
        }

        public static bool InputInt32(string Caption, string Text, ref int value, int min, int max)
        {
            int val = value;
            string s_val = value.ToString();
            bool OKVal;
            do
            {
                OKVal = true;
                if (!Query(Caption, Text, ref s_val)) return false;
                if (!Int32.TryParse(s_val, out val))
                {
                    MessageBox.Show("Требуется число в диапазоне " + min.ToString() + "..." + max.ToString() + " !");
                    OKVal = false;
                }
                else
                {
                    if (val < min || val > max)
                    {
                        MessageBox.Show("Требуется число в диапазоне " + min.ToString() + "..." + max.ToString() + " !");
                        OKVal = false;
                    }
                }
            } while (!OKVal);
            value = val;
            return true;
        }

        public static bool InputDouble(string Caption, string Text, ref double value, double min, double max)
        {
            double val = value;
            string s_val = value.ToString(CultureInfo.InvariantCulture);
            bool OKVal;
            do
            {
                OKVal = true;
                if (!Query(Caption, Text, ref s_val)) return false;
                if (!Double.TryParse(s_val, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                {
                    MessageBox.Show("Требуется число в диапазоне " + min.ToString(CultureInfo.InvariantCulture) + "..." + max.ToString(CultureInfo.InvariantCulture) + " !");
                    OKVal = false;
                }
                else
                {
                    if (val < min || val > max)
                    {
                        MessageBox.Show("Требуется число в диапазоне " + min.ToString(CultureInfo.InvariantCulture) + "..." + max.ToString(CultureInfo.InvariantCulture) + " !");
                        OKVal = false;
                    }
                }
            } while (!OKVal);
            value = val;
            return true;
        }
    }
}
