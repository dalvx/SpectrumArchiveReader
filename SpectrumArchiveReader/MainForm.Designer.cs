namespace SpectrumArchiveReader
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.C_BuildFileTable = new System.Windows.Forms.Button();
            this.C_Log = new System.Windows.Forms.RichTextBox();
            this.C_Abort = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.C_BuildDiskTable = new System.Windows.Forms.Button();
            this.C_BuildMaps = new System.Windows.Forms.Button();
            this.C_Test = new System.Windows.Forms.Button();
            this.C_SearchInFiles = new System.Windows.Forms.Button();
            this.C_SearchBytesInFiles = new System.Windows.Forms.Button();
            this.C_FixTrd = new System.Windows.Forms.Button();
            this.C_BuildDuplicateMaps = new System.Windows.Forms.Button();
            this.C_BuildSectorContentMaps = new System.Windows.Forms.Button();
            this.C_DataRate = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.C_GetVersion = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.C_TB_RsCount = new System.Windows.Forms.TextBox();
            this.C_TB_RsSector = new System.Windows.Forms.TextBox();
            this.C_TB_RsTrack = new System.Windows.Forms.TextBox();
            this.C_MeasureRotationSpeed = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.C_Help = new System.Windows.Forms.Button();
            this.C_ScanDisk = new System.Windows.Forms.Button();
            this.C_SaveDiskFormat = new System.Windows.Forms.Button();
            this.C_NewDiskFormat = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.C_UIRefreshPeriod = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.C_LoadDiskFormat = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // C_BuildFileTable
            // 
            this.C_BuildFileTable.Location = new System.Drawing.Point(8, 65);
            this.C_BuildFileTable.Name = "C_BuildFileTable";
            this.C_BuildFileTable.Size = new System.Drawing.Size(120, 23);
            this.C_BuildFileTable.TabIndex = 3;
            this.C_BuildFileTable.Text = "Build File Table";
            this.C_BuildFileTable.UseVisualStyleBackColor = true;
            this.C_BuildFileTable.Click += new System.EventHandler(this.BuildFileTable_Click);
            // 
            // C_Log
            // 
            this.C_Log.Location = new System.Drawing.Point(0, 389);
            this.C_Log.Name = "C_Log";
            this.C_Log.Size = new System.Drawing.Size(1041, 201);
            this.C_Log.TabIndex = 5;
            this.C_Log.Text = "";
            this.C_Log.WordWrap = false;
            // 
            // C_Abort
            // 
            this.C_Abort.Location = new System.Drawing.Point(537, 218);
            this.C_Abort.Name = "C_Abort";
            this.C_Abort.Size = new System.Drawing.Size(75, 23);
            this.C_Abort.TabIndex = 13;
            this.C_Abort.Text = "Abort";
            this.C_Abort.UseVisualStyleBackColor = true;
            this.C_Abort.Click += new System.EventHandler(this.C_Abort_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 25;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // C_BuildDiskTable
            // 
            this.C_BuildDiskTable.Location = new System.Drawing.Point(8, 39);
            this.C_BuildDiskTable.Name = "C_BuildDiskTable";
            this.C_BuildDiskTable.Size = new System.Drawing.Size(120, 23);
            this.C_BuildDiskTable.TabIndex = 2;
            this.C_BuildDiskTable.Text = "Build Disk Table";
            this.C_BuildDiskTable.UseVisualStyleBackColor = true;
            this.C_BuildDiskTable.Click += new System.EventHandler(this.BuildDiskTable_Click);
            // 
            // C_BuildMaps
            // 
            this.C_BuildMaps.Location = new System.Drawing.Point(8, 13);
            this.C_BuildMaps.Name = "C_BuildMaps";
            this.C_BuildMaps.Size = new System.Drawing.Size(120, 23);
            this.C_BuildMaps.TabIndex = 1;
            this.C_BuildMaps.Text = "Build Maps";
            this.C_BuildMaps.UseVisualStyleBackColor = true;
            this.C_BuildMaps.Click += new System.EventHandler(this.BuildMaps_Click);
            // 
            // C_Test
            // 
            this.C_Test.Location = new System.Drawing.Point(21, 227);
            this.C_Test.Name = "C_Test";
            this.C_Test.Size = new System.Drawing.Size(120, 23);
            this.C_Test.TabIndex = 31;
            this.C_Test.Text = "Test";
            this.C_Test.UseVisualStyleBackColor = true;
            this.C_Test.Click += new System.EventHandler(this.Test_Click);
            // 
            // C_SearchInFiles
            // 
            this.C_SearchInFiles.Location = new System.Drawing.Point(21, 174);
            this.C_SearchInFiles.Name = "C_SearchInFiles";
            this.C_SearchInFiles.Size = new System.Drawing.Size(120, 23);
            this.C_SearchInFiles.TabIndex = 32;
            this.C_SearchInFiles.Text = "Search In Files";
            this.C_SearchInFiles.UseVisualStyleBackColor = true;
            this.C_SearchInFiles.Click += new System.EventHandler(this.SearchInFiles_Click);
            // 
            // C_SearchBytesInFiles
            // 
            this.C_SearchBytesInFiles.Location = new System.Drawing.Point(21, 198);
            this.C_SearchBytesInFiles.Name = "C_SearchBytesInFiles";
            this.C_SearchBytesInFiles.Size = new System.Drawing.Size(120, 23);
            this.C_SearchBytesInFiles.TabIndex = 33;
            this.C_SearchBytesInFiles.Text = "Search Bytes In Files";
            this.C_SearchBytesInFiles.UseVisualStyleBackColor = true;
            this.C_SearchBytesInFiles.Click += new System.EventHandler(this.SearchBytesInFiles_Click);
            // 
            // C_FixTrd
            // 
            this.C_FixTrd.Location = new System.Drawing.Point(8, 90);
            this.C_FixTrd.Name = "C_FixTrd";
            this.C_FixTrd.Size = new System.Drawing.Size(120, 23);
            this.C_FixTrd.TabIndex = 34;
            this.C_FixTrd.Text = "Fix TRD";
            this.C_FixTrd.UseVisualStyleBackColor = true;
            this.C_FixTrd.Click += new System.EventHandler(this.FixTrd_Click);
            // 
            // C_BuildDuplicateMaps
            // 
            this.C_BuildDuplicateMaps.Location = new System.Drawing.Point(134, 13);
            this.C_BuildDuplicateMaps.Name = "C_BuildDuplicateMaps";
            this.C_BuildDuplicateMaps.Size = new System.Drawing.Size(144, 23);
            this.C_BuildDuplicateMaps.TabIndex = 35;
            this.C_BuildDuplicateMaps.Text = "Build Duplicate Maps";
            this.C_BuildDuplicateMaps.UseVisualStyleBackColor = true;
            this.C_BuildDuplicateMaps.Click += new System.EventHandler(this.BuildDuplicateMaps_Click);
            // 
            // C_BuildSectorContentMaps
            // 
            this.C_BuildSectorContentMaps.Location = new System.Drawing.Point(134, 38);
            this.C_BuildSectorContentMaps.Name = "C_BuildSectorContentMaps";
            this.C_BuildSectorContentMaps.Size = new System.Drawing.Size(144, 23);
            this.C_BuildSectorContentMaps.TabIndex = 36;
            this.C_BuildSectorContentMaps.Text = "Build Sector Content Maps";
            this.C_BuildSectorContentMaps.UseVisualStyleBackColor = true;
            this.C_BuildSectorContentMaps.Click += new System.EventHandler(this.BuildSectorContentMaps_Click);
            // 
            // C_DataRate
            // 
            this.C_DataRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.C_DataRate.FormattingEnabled = true;
            this.C_DataRate.Items.AddRange(new object[] {
            "500K",
            "300K",
            "250K",
            "1M"});
            this.C_DataRate.Location = new System.Drawing.Point(8, 27);
            this.C_DataRate.Name = "C_DataRate";
            this.C_DataRate.Size = new System.Drawing.Size(121, 21);
            this.C_DataRate.TabIndex = 37;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 38;
            this.label1.Text = "Data Rate";
            // 
            // C_GetVersion
            // 
            this.C_GetVersion.Location = new System.Drawing.Point(134, 64);
            this.C_GetVersion.Name = "C_GetVersion";
            this.C_GetVersion.Size = new System.Drawing.Size(144, 23);
            this.C_GetVersion.TabIndex = 59;
            this.C_GetVersion.Text = "Driver Version";
            this.C_GetVersion.UseVisualStyleBackColor = true;
            this.C_GetVersion.Click += new System.EventHandler(this.GetVersion_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.C_TB_RsCount);
            this.groupBox1.Controls.Add(this.C_TB_RsSector);
            this.groupBox1.Controls.Add(this.C_TB_RsTrack);
            this.groupBox1.Controls.Add(this.C_MeasureRotationSpeed);
            this.groupBox1.Controls.Add(this.label32);
            this.groupBox1.Controls.Add(this.label31);
            this.groupBox1.Controls.Add(this.label23);
            this.groupBox1.Location = new System.Drawing.Point(12, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(161, 100);
            this.groupBox1.TabIndex = 60;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Скорость вращения диска";
            // 
            // C_TB_RsCount
            // 
            this.C_TB_RsCount.Location = new System.Drawing.Point(127, 43);
            this.C_TB_RsCount.Name = "C_TB_RsCount";
            this.C_TB_RsCount.Size = new System.Drawing.Size(28, 20);
            this.C_TB_RsCount.TabIndex = 6;
            this.C_TB_RsCount.Text = "5";
            // 
            // C_TB_RsSector
            // 
            this.C_TB_RsSector.Location = new System.Drawing.Point(126, 17);
            this.C_TB_RsSector.Name = "C_TB_RsSector";
            this.C_TB_RsSector.Size = new System.Drawing.Size(28, 20);
            this.C_TB_RsSector.TabIndex = 5;
            this.C_TB_RsSector.Text = "0";
            // 
            // C_TB_RsTrack
            // 
            this.C_TB_RsTrack.Location = new System.Drawing.Point(44, 17);
            this.C_TB_RsTrack.Name = "C_TB_RsTrack";
            this.C_TB_RsTrack.Size = new System.Drawing.Size(28, 20);
            this.C_TB_RsTrack.TabIndex = 4;
            this.C_TB_RsTrack.Text = "10";
            // 
            // C_MeasureRotationSpeed
            // 
            this.C_MeasureRotationSpeed.Location = new System.Drawing.Point(9, 69);
            this.C_MeasureRotationSpeed.Name = "C_MeasureRotationSpeed";
            this.C_MeasureRotationSpeed.Size = new System.Drawing.Size(75, 23);
            this.C_MeasureRotationSpeed.TabIndex = 3;
            this.C_MeasureRotationSpeed.Text = "Измерить";
            this.C_MeasureRotationSpeed.UseVisualStyleBackColor = true;
            this.C_MeasureRotationSpeed.Click += new System.EventHandler(this.MeasureRotationSpeed);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(50, 49);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(76, 13);
            this.label32.TabIndex = 2;
            this.label32.Text = "Число чтений";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(84, 22);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(43, 13);
            this.label31.TabIndex = 1;
            this.label31.Text = "Сектор";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 20);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(32, 13);
            this.label23.TabIndex = 0;
            this.label23.Text = "Трек";
            // 
            // C_Help
            // 
            this.C_Help.Location = new System.Drawing.Point(134, 90);
            this.C_Help.Name = "C_Help";
            this.C_Help.Size = new System.Drawing.Size(75, 23);
            this.C_Help.TabIndex = 66;
            this.C_Help.Text = "Help";
            this.C_Help.UseVisualStyleBackColor = true;
            this.C_Help.Click += new System.EventHandler(this.C_Help_Click);
            // 
            // C_ScanDisk
            // 
            this.C_ScanDisk.Location = new System.Drawing.Point(537, 110);
            this.C_ScanDisk.Name = "C_ScanDisk";
            this.C_ScanDisk.Size = new System.Drawing.Size(75, 23);
            this.C_ScanDisk.TabIndex = 72;
            this.C_ScanDisk.Text = "Scan Disk";
            this.C_ScanDisk.UseVisualStyleBackColor = true;
            this.C_ScanDisk.Click += new System.EventHandler(this.C_ScanDisk_Click);
            // 
            // C_SaveDiskFormat
            // 
            this.C_SaveDiskFormat.Location = new System.Drawing.Point(537, 138);
            this.C_SaveDiskFormat.Name = "C_SaveDiskFormat";
            this.C_SaveDiskFormat.Size = new System.Drawing.Size(75, 23);
            this.C_SaveDiskFormat.TabIndex = 73;
            this.C_SaveDiskFormat.Text = "Save";
            this.C_SaveDiskFormat.UseVisualStyleBackColor = true;
            this.C_SaveDiskFormat.Click += new System.EventHandler(this.C_SaveDiskFormat_Click);
            // 
            // C_NewDiskFormat
            // 
            this.C_NewDiskFormat.Location = new System.Drawing.Point(537, 163);
            this.C_NewDiskFormat.Name = "C_NewDiskFormat";
            this.C_NewDiskFormat.Size = new System.Drawing.Size(75, 23);
            this.C_NewDiskFormat.TabIndex = 74;
            this.C_NewDiskFormat.Text = "New";
            this.C_NewDiskFormat.UseVisualStyleBackColor = true;
            this.C_NewDiskFormat.Click += new System.EventHandler(this.C_NewDiskFormat_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(0, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1041, 384);
            this.tabControl1.TabIndex = 77;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.C_UIRefreshPeriod);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.C_BuildDuplicateMaps);
            this.tabPage1.Controls.Add(this.C_BuildFileTable);
            this.tabPage1.Controls.Add(this.C_BuildDiskTable);
            this.tabPage1.Controls.Add(this.C_BuildMaps);
            this.tabPage1.Controls.Add(this.C_FixTrd);
            this.tabPage1.Controls.Add(this.C_BuildSectorContentMaps);
            this.tabPage1.Controls.Add(this.C_GetVersion);
            this.tabPage1.Controls.Add(this.C_Help);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1033, 358);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Various";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // C_UIRefreshPeriod
            // 
            this.C_UIRefreshPeriod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.C_UIRefreshPeriod.FormattingEnabled = true;
            this.C_UIRefreshPeriod.Items.AddRange(new object[] {
            "16",
            "20",
            "32",
            "40",
            "64",
            "80",
            "100",
            "128"});
            this.C_UIRefreshPeriod.Location = new System.Drawing.Point(124, 167);
            this.C_UIRefreshPeriod.Name = "C_UIRefreshPeriod";
            this.C_UIRefreshPeriod.Size = new System.Drawing.Size(121, 21);
            this.C_UIRefreshPeriod.TabIndex = 68;
            this.C_UIRefreshPeriod.SelectedIndexChanged += new System.EventHandler(this.C_UIRefreshPeriod_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 175);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "UI Refresh Period, ms";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.C_LoadDiskFormat);
            this.tabPage2.Controls.Add(this.C_SearchBytesInFiles);
            this.tabPage2.Controls.Add(this.C_DataRate);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.C_SearchInFiles);
            this.tabPage2.Controls.Add(this.C_Test);
            this.tabPage2.Controls.Add(this.C_NewDiskFormat);
            this.tabPage2.Controls.Add(this.C_Abort);
            this.tabPage2.Controls.Add(this.C_SaveDiskFormat);
            this.tabPage2.Controls.Add(this.C_ScanDisk);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1033, 358);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "WRK";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // C_LoadDiskFormat
            // 
            this.C_LoadDiskFormat.Location = new System.Drawing.Point(618, 163);
            this.C_LoadDiskFormat.Name = "C_LoadDiskFormat";
            this.C_LoadDiskFormat.Size = new System.Drawing.Size(75, 23);
            this.C_LoadDiskFormat.TabIndex = 75;
            this.C_LoadDiskFormat.Text = "Load";
            this.C_LoadDiskFormat.UseVisualStyleBackColor = true;
            this.C_LoadDiskFormat.Click += new System.EventHandler(this.C_LoadDiskFormat_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1033, 358);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "TR-DOS";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1033, 358);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "CP/M";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(1033, 358);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "IS-DOS";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1042, 594);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.C_Log);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1058, 632);
            this.Name = "MainForm";
            this.Text = "Spectrum Archive Reader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button C_BuildFileTable;
        private System.Windows.Forms.RichTextBox C_Log;
        private System.Windows.Forms.Button C_Abort;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button C_BuildDiskTable;
        private System.Windows.Forms.Button C_BuildMaps;
        private System.Windows.Forms.Button C_Test;
        private System.Windows.Forms.Button C_SearchInFiles;
        private System.Windows.Forms.Button C_SearchBytesInFiles;
        private System.Windows.Forms.Button C_FixTrd;
        private System.Windows.Forms.Button C_BuildDuplicateMaps;
        private System.Windows.Forms.Button C_BuildSectorContentMaps;
        private System.Windows.Forms.ComboBox C_DataRate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button C_GetVersion;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox C_TB_RsCount;
        private System.Windows.Forms.TextBox C_TB_RsSector;
        private System.Windows.Forms.TextBox C_TB_RsTrack;
        private System.Windows.Forms.Button C_MeasureRotationSpeed;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Button C_Help;
        private System.Windows.Forms.Button C_ScanDisk;
        private System.Windows.Forms.Button C_SaveDiskFormat;
        private System.Windows.Forms.Button C_NewDiskFormat;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.ComboBox C_UIRefreshPeriod;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button C_LoadDiskFormat;
        private System.Windows.Forms.TabPage tabPage5;
    }
}

