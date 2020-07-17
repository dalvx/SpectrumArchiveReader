namespace SpectrumArchiveReader
{
    partial class CatalogueForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.C_Close = new System.Windows.Forms.Button();
            this.C_SaveAsTxt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox1.Location = new System.Drawing.Point(0, -1);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(375, 267);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // C_Close
            // 
            this.C_Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.C_Close.Location = new System.Drawing.Point(202, 272);
            this.C_Close.Name = "C_Close";
            this.C_Close.Size = new System.Drawing.Size(75, 23);
            this.C_Close.TabIndex = 1;
            this.C_Close.Text = "Close";
            this.C_Close.UseVisualStyleBackColor = true;
            this.C_Close.Click += new System.EventHandler(this.C_Close_Click);
            // 
            // C_SaveAsTxt
            // 
            this.C_SaveAsTxt.Location = new System.Drawing.Point(41, 272);
            this.C_SaveAsTxt.Name = "C_SaveAsTxt";
            this.C_SaveAsTxt.Size = new System.Drawing.Size(125, 23);
            this.C_SaveAsTxt.TabIndex = 2;
            this.C_SaveAsTxt.Text = "Save File List As txt";
            this.C_SaveAsTxt.UseVisualStyleBackColor = true;
            this.C_SaveAsTxt.Click += new System.EventHandler(this.C_SaveAsTxt_Click);
            // 
            // CatalogueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.C_Close;
            this.ClientSize = new System.Drawing.Size(553, 597);
            this.Controls.Add(this.C_SaveAsTxt);
            this.Controls.Add(this.C_Close);
            this.Controls.Add(this.richTextBox1);
            this.Name = "CatalogueForm";
            this.Text = "Catalogue Form";
            this.Resize += new System.EventHandler(this.Catalogue_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button C_Close;
        private System.Windows.Forms.Button C_SaveAsTxt;
    }
}