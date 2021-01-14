namespace SpectrumArchiveReader
{
    partial class SectorContentsForm
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
            this.C_Close = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.C_PrevSector = new System.Windows.Forms.Button();
            this.C_NextSector = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TrackLV = new System.Windows.Forms.Label();
            this.LabelX = new System.Windows.Forms.Label();
            this.SectorLV = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SizeLV = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // C_Close
            // 
            this.C_Close.Location = new System.Drawing.Point(463, 324);
            this.C_Close.Name = "C_Close";
            this.C_Close.Size = new System.Drawing.Size(75, 23);
            this.C_Close.TabIndex = 0;
            this.C_Close.Text = "Close";
            this.C_Close.UseVisualStyleBackColor = true;
            this.C_Close.Click += new System.EventHandler(this.Close_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 24);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(100, 96);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // C_PrevSector
            // 
            this.C_PrevSector.Location = new System.Drawing.Point(194, 324);
            this.C_PrevSector.Name = "C_PrevSector";
            this.C_PrevSector.Size = new System.Drawing.Size(75, 23);
            this.C_PrevSector.TabIndex = 2;
            this.C_PrevSector.Text = "Prev Sector";
            this.C_PrevSector.UseVisualStyleBackColor = true;
            this.C_PrevSector.Click += new System.EventHandler(this.PrevSector_Click);
            // 
            // C_NextSector
            // 
            this.C_NextSector.Location = new System.Drawing.Point(275, 324);
            this.C_NextSector.Name = "C_NextSector";
            this.C_NextSector.Size = new System.Drawing.Size(75, 23);
            this.C_NextSector.TabIndex = 3;
            this.C_NextSector.Text = "Next Sector";
            this.C_NextSector.UseVisualStyleBackColor = true;
            this.C_NextSector.Click += new System.EventHandler(this.NextSector_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Track:";
            // 
            // TrackLV
            // 
            this.TrackLV.AutoSize = true;
            this.TrackLV.Location = new System.Drawing.Point(53, 8);
            this.TrackLV.Name = "TrackLV";
            this.TrackLV.Size = new System.Drawing.Size(16, 13);
            this.TrackLV.TabIndex = 5;
            this.TrackLV.Text = "...";
            // 
            // LabelX
            // 
            this.LabelX.AutoSize = true;
            this.LabelX.Location = new System.Drawing.Point(100, 8);
            this.LabelX.Name = "LabelX";
            this.LabelX.Size = new System.Drawing.Size(41, 13);
            this.LabelX.TabIndex = 6;
            this.LabelX.Text = "Sector:";
            // 
            // SectorLV
            // 
            this.SectorLV.AutoSize = true;
            this.SectorLV.Location = new System.Drawing.Point(147, 8);
            this.SectorLV.Name = "SectorLV";
            this.SectorLV.Size = new System.Drawing.Size(16, 13);
            this.SectorLV.TabIndex = 7;
            this.SectorLV.Text = "...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(182, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Size:";
            // 
            // SizeLV
            // 
            this.SizeLV.AutoSize = true;
            this.SizeLV.Location = new System.Drawing.Point(218, 9);
            this.SizeLV.Name = "SizeLV";
            this.SizeLV.Size = new System.Drawing.Size(16, 13);
            this.SizeLV.TabIndex = 9;
            this.SizeLV.Text = "...";
            // 
            // SectorContentsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 381);
            this.Controls.Add(this.SizeLV);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SectorLV);
            this.Controls.Add(this.LabelX);
            this.Controls.Add(this.TrackLV);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.C_NextSector);
            this.Controls.Add(this.C_PrevSector);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.C_Close);
            this.Name = "SectorContentsForm";
            this.Text = "SectorContents";
            this.Resize += new System.EventHandler(this.SectorContents_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button C_Close;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button C_PrevSector;
        private System.Windows.Forms.Button C_NextSector;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label TrackLV;
        private System.Windows.Forms.Label LabelX;
        private System.Windows.Forms.Label SectorLV;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label SizeLV;
    }
}