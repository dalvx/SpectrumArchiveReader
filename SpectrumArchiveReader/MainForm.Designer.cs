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
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.C_HtNumberOfReads = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.C_HtRandomReadOn = new System.Windows.Forms.CheckBox();
            this.C_HtTimeout = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.C_HtStopOnNthFail = new System.Windows.Forms.TextBox();
            this.C_HtCpmCheckBox = new System.Windows.Forms.CheckBox();
            this.C_HtIsDosCheckBox = new System.Windows.Forms.CheckBox();
            this.C_HtTrDosCheckBox = new System.Windows.Forms.CheckBox();
            this.C_HtSectorReadAttempts = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.C_HtSave = new System.Windows.Forms.Button();
            this.C_HtDefaultImageSize = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.C_HtMaxTracks = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.C_HtAbort = new System.Windows.Forms.Button();
            this.C_RepeatReading = new System.Windows.Forms.Button();
            this.C_NewDisk = new System.Windows.Forms.Button();
            this.C_HtDataRate = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.C_FileTypeCpm = new System.Windows.Forms.ComboBox();
            this.C_FilePatternCpm = new System.Windows.Forms.TextBox();
            this.C_SelectSavePathCpm = new System.Windows.Forms.Button();
            this.C_FileTypeIsDos = new System.Windows.Forms.ComboBox();
            this.C_FilePatternIsDos = new System.Windows.Forms.TextBox();
            this.C_SelectSavePathIsDos = new System.Windows.Forms.Button();
            this.C_FileTypeTrDos = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.C_FilePatternTrDos = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.C_SelectSavePathTrDos = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.tabControl1.Controls.Add(this.tabPage6);
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
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.C_HtNumberOfReads);
            this.tabPage6.Controls.Add(this.label3);
            this.tabPage6.Controls.Add(this.groupBox2);
            this.tabPage6.Controls.Add(this.C_HtCpmCheckBox);
            this.tabPage6.Controls.Add(this.C_HtIsDosCheckBox);
            this.tabPage6.Controls.Add(this.C_HtTrDosCheckBox);
            this.tabPage6.Controls.Add(this.C_HtSectorReadAttempts);
            this.tabPage6.Controls.Add(this.label7);
            this.tabPage6.Controls.Add(this.label6);
            this.tabPage6.Controls.Add(this.C_HtSave);
            this.tabPage6.Controls.Add(this.C_HtDefaultImageSize);
            this.tabPage6.Controls.Add(this.label14);
            this.tabPage6.Controls.Add(this.C_HtMaxTracks);
            this.tabPage6.Controls.Add(this.label13);
            this.tabPage6.Controls.Add(this.C_HtAbort);
            this.tabPage6.Controls.Add(this.C_RepeatReading);
            this.tabPage6.Controls.Add(this.C_NewDisk);
            this.tabPage6.Controls.Add(this.C_HtDataRate);
            this.tabPage6.Controls.Add(this.label12);
            this.tabPage6.Controls.Add(this.C_FileTypeCpm);
            this.tabPage6.Controls.Add(this.C_FilePatternCpm);
            this.tabPage6.Controls.Add(this.C_SelectSavePathCpm);
            this.tabPage6.Controls.Add(this.C_FileTypeIsDos);
            this.tabPage6.Controls.Add(this.C_FilePatternIsDos);
            this.tabPage6.Controls.Add(this.C_SelectSavePathIsDos);
            this.tabPage6.Controls.Add(this.C_FileTypeTrDos);
            this.tabPage6.Controls.Add(this.label5);
            this.tabPage6.Controls.Add(this.C_FilePatternTrDos);
            this.tabPage6.Controls.Add(this.label4);
            this.tabPage6.Controls.Add(this.C_SelectSavePathTrDos);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(1033, 358);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "HT";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // C_HtNumberOfReads
            // 
            this.C_HtNumberOfReads.Location = new System.Drawing.Point(518, 109);
            this.C_HtNumberOfReads.Name = "C_HtNumberOfReads";
            this.C_HtNumberOfReads.Size = new System.Drawing.Size(34, 20);
            this.C_HtNumberOfReads.TabIndex = 60;
            this.C_HtNumberOfReads.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(422, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 59;
            this.label3.Text = "Number of Reads";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.C_HtRandomReadOn);
            this.groupBox2.Controls.Add(this.C_HtTimeout);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.C_HtStopOnNthFail);
            this.groupBox2.Location = new System.Drawing.Point(558, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(136, 93);
            this.groupBox2.TabIndex = 58;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Read Random Sectors";
            // 
            // C_HtRandomReadOn
            // 
            this.C_HtRandomReadOn.AutoSize = true;
            this.C_HtRandomReadOn.Location = new System.Drawing.Point(6, 17);
            this.C_HtRandomReadOn.Name = "C_HtRandomReadOn";
            this.C_HtRandomReadOn.Size = new System.Drawing.Size(63, 17);
            this.C_HtRandomReadOn.TabIndex = 55;
            this.C_HtRandomReadOn.Text = "Turn on";
            this.C_HtRandomReadOn.UseVisualStyleBackColor = true;
            this.C_HtRandomReadOn.CheckedChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_HtTimeout
            // 
            this.C_HtTimeout.Location = new System.Drawing.Point(57, 40);
            this.C_HtTimeout.Name = "C_HtTimeout";
            this.C_HtTimeout.Size = new System.Drawing.Size(72, 20);
            this.C_HtTimeout.TabIndex = 52;
            this.C_HtTimeout.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 44);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 13);
            this.label9.TabIndex = 51;
            this.label9.Text = "Timeout";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 73);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 13);
            this.label10.TabIndex = 53;
            this.label10.Text = "Stop on Nth Fail";
            // 
            // C_HtStopOnNthFail
            // 
            this.C_HtStopOnNthFail.Location = new System.Drawing.Point(95, 66);
            this.C_HtStopOnNthFail.Name = "C_HtStopOnNthFail";
            this.C_HtStopOnNthFail.Size = new System.Drawing.Size(34, 20);
            this.C_HtStopOnNthFail.TabIndex = 54;
            this.C_HtStopOnNthFail.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_HtCpmCheckBox
            // 
            this.C_HtCpmCheckBox.AutoSize = true;
            this.C_HtCpmCheckBox.Location = new System.Drawing.Point(3, 77);
            this.C_HtCpmCheckBox.Name = "C_HtCpmCheckBox";
            this.C_HtCpmCheckBox.Size = new System.Drawing.Size(54, 17);
            this.C_HtCpmCheckBox.TabIndex = 57;
            this.C_HtCpmCheckBox.Text = "CP/M";
            this.C_HtCpmCheckBox.UseVisualStyleBackColor = true;
            this.C_HtCpmCheckBox.CheckedChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_HtIsDosCheckBox
            // 
            this.C_HtIsDosCheckBox.AutoSize = true;
            this.C_HtIsDosCheckBox.Location = new System.Drawing.Point(3, 51);
            this.C_HtIsDosCheckBox.Name = "C_HtIsDosCheckBox";
            this.C_HtIsDosCheckBox.Size = new System.Drawing.Size(62, 17);
            this.C_HtIsDosCheckBox.TabIndex = 56;
            this.C_HtIsDosCheckBox.Text = "IS-DOS";
            this.C_HtIsDosCheckBox.UseVisualStyleBackColor = true;
            this.C_HtIsDosCheckBox.CheckedChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_HtTrDosCheckBox
            // 
            this.C_HtTrDosCheckBox.AutoSize = true;
            this.C_HtTrDosCheckBox.Location = new System.Drawing.Point(3, 25);
            this.C_HtTrDosCheckBox.Name = "C_HtTrDosCheckBox";
            this.C_HtTrDosCheckBox.Size = new System.Drawing.Size(67, 17);
            this.C_HtTrDosCheckBox.TabIndex = 55;
            this.C_HtTrDosCheckBox.Text = "TR-DOS";
            this.C_HtTrDosCheckBox.UseVisualStyleBackColor = true;
            this.C_HtTrDosCheckBox.CheckedChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_HtSectorReadAttempts
            // 
            this.C_HtSectorReadAttempts.Location = new System.Drawing.Point(518, 31);
            this.C_HtSectorReadAttempts.Name = "C_HtSectorReadAttempts";
            this.C_HtSectorReadAttempts.Size = new System.Drawing.Size(34, 20);
            this.C_HtSectorReadAttempts.TabIndex = 50;
            this.C_HtSectorReadAttempts.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(401, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 13);
            this.label7.TabIndex = 49;
            this.label7.Text = "Sector Read Attempts";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(74, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 13);
            this.label6.TabIndex = 48;
            this.label6.Text = "Save Directory";
            // 
            // C_HtSave
            // 
            this.C_HtSave.Location = new System.Drawing.Point(198, 132);
            this.C_HtSave.Name = "C_HtSave";
            this.C_HtSave.Size = new System.Drawing.Size(75, 23);
            this.C_HtSave.TabIndex = 47;
            this.C_HtSave.Text = "Save";
            this.C_HtSave.UseVisualStyleBackColor = true;
            this.C_HtSave.Click += new System.EventHandler(this.C_HtSave_Click);
            // 
            // C_HtDefaultImageSize
            // 
            this.C_HtDefaultImageSize.Location = new System.Drawing.Point(518, 57);
            this.C_HtDefaultImageSize.Name = "C_HtDefaultImageSize";
            this.C_HtDefaultImageSize.Size = new System.Drawing.Size(34, 20);
            this.C_HtDefaultImageSize.TabIndex = 46;
            this.C_HtDefaultImageSize.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(377, 64);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(135, 13);
            this.label14.TabIndex = 45;
            this.label14.Text = "Default Image Size, Tracks";
            // 
            // C_HtMaxTracks
            // 
            this.C_HtMaxTracks.Location = new System.Drawing.Point(518, 83);
            this.C_HtMaxTracks.Name = "C_HtMaxTracks";
            this.C_HtMaxTracks.Size = new System.Drawing.Size(34, 20);
            this.C_HtMaxTracks.TabIndex = 44;
            this.C_HtMaxTracks.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(449, 90);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(63, 13);
            this.label13.TabIndex = 43;
            this.label13.Text = "Max Tracks";
            // 
            // C_HtAbort
            // 
            this.C_HtAbort.Location = new System.Drawing.Point(198, 161);
            this.C_HtAbort.Name = "C_HtAbort";
            this.C_HtAbort.Size = new System.Drawing.Size(75, 23);
            this.C_HtAbort.TabIndex = 41;
            this.C_HtAbort.Text = "Abort (Esc)";
            this.C_HtAbort.UseVisualStyleBackColor = true;
            this.C_HtAbort.Click += new System.EventHandler(this.C_HtAbort_Click);
            // 
            // C_RepeatReading
            // 
            this.C_RepeatReading.Location = new System.Drawing.Point(8, 161);
            this.C_RepeatReading.Name = "C_RepeatReading";
            this.C_RepeatReading.Size = new System.Drawing.Size(151, 23);
            this.C_RepeatReading.TabIndex = 40;
            this.C_RepeatReading.Text = "Repeat Reading (F5)";
            this.C_RepeatReading.UseVisualStyleBackColor = true;
            this.C_RepeatReading.Click += new System.EventHandler(this.C_RepeatReading_Click);
            // 
            // C_NewDisk
            // 
            this.C_NewDisk.FlatAppearance.BorderColor = System.Drawing.Color.Fuchsia;
            this.C_NewDisk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Red;
            this.C_NewDisk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Lime;
            this.C_NewDisk.Location = new System.Drawing.Point(8, 132);
            this.C_NewDisk.Name = "C_NewDisk";
            this.C_NewDisk.Size = new System.Drawing.Size(151, 23);
            this.C_NewDisk.TabIndex = 39;
            this.C_NewDisk.Text = "New Disk (F4)";
            this.C_NewDisk.UseVisualStyleBackColor = true;
            this.C_NewDisk.Click += new System.EventHandler(this.C_NewDisk_Click);
            // 
            // C_HtDataRate
            // 
            this.C_HtDataRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.C_HtDataRate.FormattingEnabled = true;
            this.C_HtDataRate.Items.AddRange(new object[] {
            "250K",
            "300K",
            "500K",
            "1M"});
            this.C_HtDataRate.Location = new System.Drawing.Point(431, 5);
            this.C_HtDataRate.Name = "C_HtDataRate";
            this.C_HtDataRate.Size = new System.Drawing.Size(121, 21);
            this.C_HtDataRate.TabIndex = 38;
            this.C_HtDataRate.SelectedIndexChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(369, 13);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 21;
            this.label12.Text = "Data Rate";
            // 
            // C_FileTypeCpm
            // 
            this.C_FileTypeCpm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.C_FileTypeCpm.FormattingEnabled = true;
            this.C_FileTypeCpm.Items.AddRange(new object[] {
            "FDI",
            "KDI"});
            this.C_FileTypeCpm.Location = new System.Drawing.Point(265, 74);
            this.C_FileTypeCpm.Name = "C_FileTypeCpm";
            this.C_FileTypeCpm.Size = new System.Drawing.Size(98, 21);
            this.C_FileTypeCpm.TabIndex = 20;
            this.C_FileTypeCpm.SelectedIndexChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_FilePatternCpm
            // 
            this.C_FilePatternCpm.Location = new System.Drawing.Point(158, 75);
            this.C_FilePatternCpm.Name = "C_FilePatternCpm";
            this.C_FilePatternCpm.Size = new System.Drawing.Size(99, 20);
            this.C_FilePatternCpm.TabIndex = 18;
            this.C_FilePatternCpm.Text = "Disk ***";
            this.C_FilePatternCpm.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_SelectSavePathCpm
            // 
            this.C_SelectSavePathCpm.Location = new System.Drawing.Point(76, 75);
            this.C_SelectSavePathCpm.Name = "C_SelectSavePathCpm";
            this.C_SelectSavePathCpm.Size = new System.Drawing.Size(75, 23);
            this.C_SelectSavePathCpm.TabIndex = 16;
            this.C_SelectSavePathCpm.Text = "Select";
            this.C_SelectSavePathCpm.UseVisualStyleBackColor = true;
            this.C_SelectSavePathCpm.Click += new System.EventHandler(this.C_SelectSavePathCpm_Click);
            // 
            // C_FileTypeIsDos
            // 
            this.C_FileTypeIsDos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.C_FileTypeIsDos.FormattingEnabled = true;
            this.C_FileTypeIsDos.Items.AddRange(new object[] {
            "FDI"});
            this.C_FileTypeIsDos.Location = new System.Drawing.Point(265, 47);
            this.C_FileTypeIsDos.Name = "C_FileTypeIsDos";
            this.C_FileTypeIsDos.Size = new System.Drawing.Size(98, 21);
            this.C_FileTypeIsDos.TabIndex = 13;
            this.C_FileTypeIsDos.SelectedIndexChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_FilePatternIsDos
            // 
            this.C_FilePatternIsDos.Location = new System.Drawing.Point(158, 48);
            this.C_FilePatternIsDos.Name = "C_FilePatternIsDos";
            this.C_FilePatternIsDos.Size = new System.Drawing.Size(99, 20);
            this.C_FilePatternIsDos.TabIndex = 11;
            this.C_FilePatternIsDos.Text = "Disk ***";
            this.C_FilePatternIsDos.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // C_SelectSavePathIsDos
            // 
            this.C_SelectSavePathIsDos.Location = new System.Drawing.Point(76, 48);
            this.C_SelectSavePathIsDos.Name = "C_SelectSavePathIsDos";
            this.C_SelectSavePathIsDos.Size = new System.Drawing.Size(75, 23);
            this.C_SelectSavePathIsDos.TabIndex = 9;
            this.C_SelectSavePathIsDos.Text = "Select";
            this.C_SelectSavePathIsDos.UseVisualStyleBackColor = true;
            this.C_SelectSavePathIsDos.Click += new System.EventHandler(this.C_SelectSavePathIsDos_Click);
            // 
            // C_FileTypeTrDos
            // 
            this.C_FileTypeTrDos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.C_FileTypeTrDos.FormattingEnabled = true;
            this.C_FileTypeTrDos.Items.AddRange(new object[] {
            "FDI",
            "TRD",
            "Modified TRD"});
            this.C_FileTypeTrDos.Location = new System.Drawing.Point(265, 20);
            this.C_FileTypeTrDos.Name = "C_FileTypeTrDos";
            this.C_FileTypeTrDos.Size = new System.Drawing.Size(98, 21);
            this.C_FileTypeTrDos.TabIndex = 6;
            this.C_FileTypeTrDos.SelectedIndexChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(262, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "File Type";
            // 
            // C_FilePatternTrDos
            // 
            this.C_FilePatternTrDos.Location = new System.Drawing.Point(158, 22);
            this.C_FilePatternTrDos.Name = "C_FilePatternTrDos";
            this.C_FilePatternTrDos.Size = new System.Drawing.Size(99, 20);
            this.C_FilePatternTrDos.TabIndex = 4;
            this.C_FilePatternTrDos.Text = "Disk ***";
            this.C_FilePatternTrDos.TextChanged += new System.EventHandler(this.C_FilePatternTrDos_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(155, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "File Name Pattern";
            // 
            // C_SelectSavePathTrDos
            // 
            this.C_SelectSavePathTrDos.Location = new System.Drawing.Point(76, 21);
            this.C_SelectSavePathTrDos.Name = "C_SelectSavePathTrDos";
            this.C_SelectSavePathTrDos.Size = new System.Drawing.Size(75, 23);
            this.C_SelectSavePathTrDos.TabIndex = 2;
            this.C_SelectSavePathTrDos.Text = "Select";
            this.C_SelectSavePathTrDos.UseVisualStyleBackColor = true;
            this.C_SelectSavePathTrDos.Click += new System.EventHandler(this.C_SelectSavePathTrDos_Click);
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
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.Button C_HtAbort;
        private System.Windows.Forms.Button C_RepeatReading;
        private System.Windows.Forms.Button C_NewDisk;
        private System.Windows.Forms.ComboBox C_HtDataRate;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox C_FileTypeCpm;
        private System.Windows.Forms.TextBox C_FilePatternCpm;
        private System.Windows.Forms.Button C_SelectSavePathCpm;
        private System.Windows.Forms.ComboBox C_FileTypeIsDos;
        private System.Windows.Forms.TextBox C_FilePatternIsDos;
        private System.Windows.Forms.Button C_SelectSavePathIsDos;
        private System.Windows.Forms.ComboBox C_FileTypeTrDos;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox C_FilePatternTrDos;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button C_SelectSavePathTrDos;
        private System.Windows.Forms.TextBox C_HtDefaultImageSize;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox C_HtMaxTracks;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button C_HtSave;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox C_HtSectorReadAttempts;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox C_HtTimeout;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox C_HtStopOnNthFail;
        private System.Windows.Forms.CheckBox C_HtCpmCheckBox;
        private System.Windows.Forms.CheckBox C_HtIsDosCheckBox;
        private System.Windows.Forms.CheckBox C_HtTrDosCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox C_HtNumberOfReads;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox C_HtRandomReadOn;
    }
}

