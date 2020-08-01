using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml;

namespace SpectrumArchiveReader
{
    public partial class MainForm : Form
    {
        public const int MaxTrack = 172;
        private const int MaxLines = 100;
        private const int MaxLinesH = 300;
        private MList<string> outputNormal = new MList<string>();
        public static CatalogueForm CatalogueForm;
        private HelpForm helpForm;
        private MList<ReaderBase> readers = new MList<ReaderBase>();
        private bool removeFunctionNameFromLog = false;
        private DiskReader diskReader;
        private MList<TrackFormat> diskFormat;
        public static bool Dev;

        private HtTabPars htTabPars;

        public class HtTabPars
        {
            public DiskImage Image;
            public ImageStatsTable StatsTable;
            public Map Map;
            public DiskReaderParams Params;
            public int MaxTracks = 172;
            public int NumberOfReads = 1;
            public string TrDosDir = null;
            public string CpmDir = null;
            public string IsDosDir = null;
            public FileType TrDosFileType = FileType.Fdi;
            public FileType CpmFileType = FileType.Fdi;
            public FileType IsDosFileType = FileType.Fdi;
            public string TrDosFileName = "D***";
            public string CpmFileName = "D***";
            public string IsDosFileName = "D***";
            public DataRate DataRate = DataRate.FD_RATE_250K;
            public bool TrDosChecked = true;
            public bool IsDosChecked = true;
            public bool CpmChecked = true;
            public int SectorReadAttempts = 1;
            public int DefaultImageSizeInTracks = 160;
            public bool Enabled = true;
            public bool Processing;
            public bool RandomReadTurnedOn;
            public TimeSpan RandomReadTimeout;
            public int RandomReadStopOnNthFail;
            public bool ParametersChanging;
            /// <summary>
            /// Имя файла куда был сохранен образ. Является также признаком того что образ был сохранен. Если образ не был сохранен, то null.
            /// </summary>
            public string CurrentFileName;
            public TrackFormat Track0Format = new TrackFormat(55);

            public static FileType[] TrDosFileTypes = { FileType.Fdi, FileType.Trd, FileType.ModifiedTrd };
            public static FileType[] IsDosFileTypes = { FileType.Fdi };
            public static FileType[] CpmFileTypes = { FileType.Fdi, FileType.Kdi };

            public static int GetFileTypeIndex(FileType fileType, FileType[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == fileType) return i;
                }
                throw new Exception();
            }

            public static int GetDataRateIndex(DataRate dataRate, DataRate[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == dataRate) return i;
                }
                throw new Exception();
            }

            public void WriteToXml(XmlTextWriter xml, string name)
            {
                xml.WriteStartElement(name);
                xml.WriteAttributeString("MaxTracks", MaxTracks.ToString());
                xml.WriteAttributeString("NumberOfReads", NumberOfReads.ToString());
                xml.WriteAttributeString("TrDosDir", TrDosDir);
                xml.WriteAttributeString("IsDosDir", IsDosDir);
                xml.WriteAttributeString("CpmDir", CpmDir);
                xml.WriteAttributeString("TrDosFileType", TrDosFileType.ToString());
                xml.WriteAttributeString("CpmFileType", CpmFileType.ToString());
                xml.WriteAttributeString("IsDosFileType", IsDosFileType.ToString());
                xml.WriteAttributeString("TrDosFileName", TrDosFileName);
                xml.WriteAttributeString("CpmFileName", CpmFileName);
                xml.WriteAttributeString("IsDosFileName", IsDosFileName);
                xml.WriteAttributeString("DataRate", DataRate.ToString());
                xml.WriteAttributeString("TrDosChecked", TrDosChecked.ToString());
                xml.WriteAttributeString("IsDosChecked", IsDosChecked.ToString());
                xml.WriteAttributeString("CpmChecked", CpmChecked.ToString());
                xml.WriteAttributeString("SectorReadAttempts", SectorReadAttempts.ToString());
                xml.WriteAttributeString("DefaultImageSizeInTracks", DefaultImageSizeInTracks.ToString());
                xml.WriteAttributeString("RandomReadTurnedOn", RandomReadTurnedOn.ToString());
                xml.WriteAttributeString("RandomReadTimeout", RandomReadTimeout.ToString());
                xml.WriteAttributeString("RandomReadStopOnNthFail", RandomReadStopOnNthFail.ToString());
                xml.WriteEndElement();
            }

            public void ReadFromXml(XmlTextReader xml)
            {
                MaxTracks = int.Parse(xml.GetAttribute("MaxTracks"));
                NumberOfReads = int.Parse(xml.GetAttribute("NumberOfReads"));
                TrDosDir = xml.GetAttribute("TrDosDir");
                IsDosDir = xml.GetAttribute("IsDosDir");
                CpmDir = xml.GetAttribute("CpmDir");
                TrDosFileType = (FileType)Enum.Parse(typeof(FileType), xml.GetAttribute("TrDosFileType"));
                CpmFileType = (FileType)Enum.Parse(typeof(FileType), xml.GetAttribute("CpmFileType"));
                IsDosFileType = (FileType)Enum.Parse(typeof(FileType), xml.GetAttribute("IsDosFileType"));
                TrDosFileName = xml.GetAttribute("TrDosFileName");
                CpmFileName = xml.GetAttribute("CpmFileName");
                IsDosFileName = xml.GetAttribute("IsDosFileName");
                DataRate = (DataRate)Enum.Parse(typeof(DataRate), xml.GetAttribute("DataRate"));
                TrDosChecked = bool.Parse(xml.GetAttribute("TrDosChecked"));
                IsDosChecked = bool.Parse(xml.GetAttribute("IsDosChecked"));
                CpmChecked = bool.Parse(xml.GetAttribute("CpmChecked"));
                SectorReadAttempts = int.Parse(xml.GetAttribute("SectorReadAttempts"));
                DefaultImageSizeInTracks = int.Parse(xml.GetAttribute("DefaultImageSizeInTracks"));
                RandomReadTurnedOn = bool.Parse(xml.GetAttribute("RandomReadTurnedOn"));
                RandomReadTimeout = TimeSpan.Parse(xml.GetAttribute("RandomReadTimeout"));
                RandomReadStopOnNthFail = int.Parse(xml.GetAttribute("RandomReadStopOnNthFail"));
            }
        }

        public MainForm()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            string dev = GetKeyStartingWith(args, "/dev");
            Dev = dev != null;
            if (!Dev)
            {
                tabControl1.Controls.RemoveAt(1);
                removeFunctionNameFromLog = true;
            }
            Log.MsgType fileAllowedMessages = 0;
            Log.MsgType windowAllowedMessages = Log.MsgType.Info | Log.MsgType.Warn | Log.MsgType.Error | Log.MsgType.Fatal;
            string logfileKey = GetKeyStartingWith(args, "/logfile");
            if (logfileKey != null) fileAllowedMessages = GetLogMsgTypeFromKey(logfileKey);
            string logWindowKey = GetKeyStartingWith(args, "/logwin");
            if (logWindowKey != null) windowAllowedMessages = GetLogMsgTypeFromKey(logWindowKey);

            if (!Log.Init(fileAllowedMessages != 0, fileAllowedMessages != 0, windowAllowedMessages != 0, outputNormal, null, fileAllowedMessages, windowAllowedMessages))
            {
                MessageBox.Show("Cannot create a log directory");
                Application.Exit();
                return;
            }

            C_UIRefreshPeriod.SelectedIndex = 2;

            DiskReaderParams cpmParams = new DiskReaderParams()
            {
                DataRate = DataRate.FD_RATE_250K,
                SectorReadAttempts = 1,
                Side = DiskSide.Both,
                UpperSideHead = UpperSideHead.Head1,
                TrackTo = 172
            };
            DiskReaderParams trDosParams = (DiskReaderParams)cpmParams.Clone();
            trDosParams.TrackLayoutAutodetect = true;
            trDosParams.ReadMode = ReadMode.Fast;
            trDosParams.UpperSideHeadAutodetect = true;
            DiskReaderParams isDosParams = (DiskReaderParams)cpmParams.Clone();

            htTabPars = new HtTabPars();
            htTabPars.StatsTable = new ImageStatsTable(tabPage6, SystemColors.Window, SystemColors.ControlText);
            htTabPars.StatsTable.SetPosition(tabPage6.Width - 290, 16);
            htTabPars.StatsTable.Repaint();
            htTabPars.Map = new Map(MaxTrack, 1024, 5, tabPage6, Color.White, htTabPars.StatsTable) { CanEditReadBounds = false };
            htTabPars.Map.SetPosition(0, 227);
            htTabPars.Map.TrackFrom = 0;
            htTabPars.Map.TrackTo = MaxTrack;
            htTabPars.Map.Repaint();

            try
            {
                string settingsFile = Path.ChangeExtension(Application.ExecutablePath, ".xml");
                if (File.Exists(settingsFile))
                {
                    using (XmlTextReader xml = new XmlTextReader(settingsFile))
                    {
                        while (xml.Read())
                        {
                            if (xml.NodeType != XmlNodeType.Element) continue;
                            switch (xml.Name)
                            {
                                case "General":
                                    tabControl1.SelectedIndex = int.Parse(xml.GetAttribute("TabIndex"));
                                    C_UIRefreshPeriod.SelectedIndex = int.Parse(xml.GetAttribute("UIRefreshPeriod"));
                                    break;

                                case "TR-DOS":
                                    trDosParams.ReadFromXml(xml);
                                    break;

                                case "CPM":
                                    cpmParams.ReadFromXml(xml);
                                    break;

                                case "IS-DOS":
                                    isDosParams.ReadFromXml(xml);
                                    break;

                                case "HT":
                                    htTabPars.ReadFromXml(xml);
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                Log.Info?.Out($"Ошибка при загрузке настроек.");
            }

            readers.Add(new TrDosReader(tabPage3, trDosParams));
            readers.Add(new CpmReader(tabPage4, cpmParams));
            readers.Add(new IsDosReader(tabPage5, isDosParams));
            for (int i = 0; i < readers.Cnt; i++)
            {
                readers[i].OperationStarted += MainForm_OperationStarted;
                readers[i].OperationCompleted += MainForm_OperationCompleted;
            }
            tabControl1.SelectedIndex = 1;

            if (!Timer.IsHighResolution)
            {
                Log.Trace?.Out("Таймер высокого разрешения недоступен.");
            }
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            HtFillParameters();
            HtSetEnabled(HtReadParameters());
        }

        private void MainForm_OperationCompleted(object sender, EventArgs e)
        {
            C_GetVersion.Enabled = true;
            C_MeasureRotationSpeed.Enabled = true;
            for (int i = 0; i < readers.Cnt; i++)
            {
                readers[i].Enabled = true;
            }
            htTabPars.Enabled = true;
            HtSetEnabled(true);
        }

        private void MainForm_OperationStarted(object sender, EventArgs e)
        {
            C_GetVersion.Enabled = false;
            C_MeasureRotationSpeed.Enabled = false;
            for (int i = 0; i < readers.Cnt; i++)
            {
                if (readers[i] != sender) readers[i].Enabled = false;
            }
            if (sender != htTabPars) htTabPars.Enabled = false;
            HtSetEnabled(true);
        }

        private bool ContainsKey(string[] args, string key)
        {
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private string GetKeyStartingWith(string[] args, string key)
        {
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith(key, StringComparison.OrdinalIgnoreCase)) return args[i];
            }
            return null;
        }

        private Log.MsgType GetLogMsgTypeFromKey(string key)
        {
            int index = key.IndexOf(':');
            if (index < 0) return Log.MsgType.All;
            key = key.ToUpperInvariant();
            Log.MsgType r = 0;
            for (int i = index + 1; i < key.Length; i++)
            {
                switch (key[i])
                {
                    case 'N':
                        return 0;

                    case 'T':
                        r |= Log.MsgType.Trace;
                        break;

                    case 'I':
                        r |= Log.MsgType.Info;
                        break;

                    case 'D':
                        r |= Log.MsgType.Debug;
                        break;

                    case 'W':
                        r |= Log.MsgType.Warn;
                        break;

                    case 'E':
                        r |= Log.MsgType.Error;
                        break;

                    case 'F':
                        r |= Log.MsgType.Fatal;
                        break;
                }
            }
            return r;
        }

        private void BuildFileTable_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""generator"" content=""Spectrum Archive Reader"">
        <title>File Table</title>
        <style>
			.r { color: red; }
        </style>
    </head>
    <body>");
            string[] files = openDialog.FileNames;
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage disk = new TrDosImage();
                disk.LoadAutodetect(files[i]);
                disk.ParseCatalogue();
                sb.Append(disk.ToHtmlTableAsFileList("		"));
            }
            sb.AppendLine(@"	</body>
</html>");
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "Html Files (*.html)|*.html|All Files (*.*)|*.*" };
            saveDialog.FileName = "File Table";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, sb.ToString());
        }

        private void ClearStrs(RichTextBox r)
        {
            string[] a = r.Lines;
            if (a.Length <= MaxLinesH) return;
            string[] b = new string[MaxLines];
            int k = a.Length - MaxLines;
            for (int i = 0; i < MaxLines; i++, k++) b[i] = a[k];
            r.Lines = b;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (outputNormal)
            {
                if (outputNormal.Cnt > 0)
                {
                    C_Log.SuspendLayout();
                    ClearStrs(C_Log);
                    for (int i = 0; i < outputNormal.Cnt; i++)
                    {
                        if (removeFunctionNameFromLog)
                        {
                            int space1 = outputNormal[i].IndexOf(' ');
                            int space2 = outputNormal[i].IndexOf(' ', space1 + 1);
                            string msgType = outputNormal[i].Substring(space1 + 1, space2 - space1 - 2);
                            if (msgType == "DEBUG" || msgType == "ERROR" || msgType == "FATAL") space2 = outputNormal[i].IndexOf(' ', space2 + 1);
                            string r = outputNormal[i].Remove(space1, space2 - space1);
                            C_Log.AppendText(r + "\n");
                        }
                        else
                        {
                            C_Log.AppendText(outputNormal[i] + "\n");
                        }
                    }
                    outputNormal.Clear();
                    C_Log.ScrollToCaret();
                    C_Log.ResumeLayout(true);
                }
            }
            for (int i = 0; i < readers.Cnt; i++)
            {
                readers[i].RefreshControls();
            }
            HtRefreshControls();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < readers.Cnt; i++)
            {
                readers[i].Abort();
            }
            using (XmlTextWriter xml = new XmlTextWriter(Path.ChangeExtension(Application.ExecutablePath, ".xml"), new UTF8Encoding()))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement("Root");
                xml.WriteStartElement("General");
                xml.WriteAttributeString("TabIndex", tabControl1.SelectedIndex.ToString());
                xml.WriteAttributeString("UIRefreshPeriod", C_UIRefreshPeriod.SelectedIndex.ToString());
                xml.WriteEndElement();
                readers[0].Params.SaveToXml(xml, "TR-DOS");
                readers[1].Params.SaveToXml(xml, "CPM");
                readers[2].Params.SaveToXml(xml, "IS-DOS");
                htTabPars.WriteToXml(xml, "HT");
                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }

        private void BuildDiskTable_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            string[] files = openDialog.FileNames;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""generator"" content=""Spectrum Archive Reader"">
        <title>Disk Table</title>
        <style>
            .statsTable tr { text-align: left; }
        </style>
    </head>
    <body>");
            string indent = "    ";
            sb.AppendLine(indent + indent + "<table>");
            sb.AppendLine(indent + indent + indent + "<tr><th>Disk</th><th>Title</th><th>Tracks</th><th>Good</th><th>Bad</th><th>Unread</th><th>Cat</th><th>Files/Damaged</th><th>Non-Zero Sectors</th></tr>");
            int totalSectors = 0;
            int totalGoodSectors = 0;
            int totalBadSectors = 0;
            int totalProcessedSectors = 0;
            int totalFiles = 0;
            int totalDamagedFiles = 0;
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage image = new TrDosImage();
                image.LoadAutodetect(files[i]);
                image.ParseCatalogue();
                string disk = image.FileNameOnly;
                string cat;
                if (image.CatIsRead)
                {
                    cat = "Y";
                }
                else
                {
                    int cnt = 0;
                    for (int j = 0; j < Math.Min(9, image.Sectors.Length); j++)
                    {
                        if (image.Sectors[j] == SectorProcessResult.Good) cnt++;
                    }
                    cat = cnt == 0 ? "N" : cnt.ToString();
                }
                sb.AppendLine(indent + indent + indent + $"<tr><td>{disk}</td><td>{image.Title}</td><td>{image.SizeTracks}</td><td>{image.GoodSectors}</td><td>{image.BadSectors}</td><td>{image.UnprocessedSectors}</td><td>{cat}</td><td>{image.Files.Cnt}/{image.DamagedFiles}</td><td>{image.NonZeroSectors}</td></tr>");
                totalSectors += image.Sectors.Length >= 2560 ? image.Sectors.Length : 2560;
                totalProcessedSectors += image.ProcessedSectors;
                totalGoodSectors += image.GoodSectors;
                totalBadSectors += image.BadSectors;
                totalFiles += image.Files.Cnt;
                totalDamagedFiles += image.DamagedFiles;
            }
            sb.AppendLine(indent + indent + "</table>");
            sb.AppendLine(indent + indent + "<table class=\"statsTable\">");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Total Disks</th><td>{files.Length}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Assumed Total Sectors</th><td>{totalSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Processed Sectors</th><td>{totalProcessedSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors</th><td>{totalGoodSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors</th><td>{totalBadSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Files</th><td>{totalFiles}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Damaged Files</th><td>{totalDamagedFiles}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors As Share Of Assumed Total</th><td>{(totalSectors > 0 ? GP.ToString((double)totalGoodSectors / totalSectors * 100, 2) : "0")} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors As Share Of Processed</th><td>{(totalProcessedSectors > 0 ? GP.ToString((double)totalGoodSectors / totalProcessedSectors * 100, 2) : "0")} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors As Share Of Total</th><td>{(totalSectors > 0 ? GP.ToString((double)totalBadSectors / totalSectors * 100, 2) : "0")} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors As Share Of Processed</th><td>{(totalProcessedSectors > 0 ? GP.ToString((double)totalBadSectors / totalProcessedSectors * 100, 2) : "0")} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Files Share</th><td>{(totalFiles > 0 ? GP.ToString((double)(totalFiles - totalDamagedFiles) / totalFiles * 100, 2) : "0")} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Damaged Files Share</th><td>{(totalFiles > 0 ? GP.ToString((double)totalDamagedFiles / totalFiles * 100, 2) : "0")} %</td></tr>");
            sb.AppendLine(indent + indent + "</table>");
            sb.AppendLine(indent + "</body>");
            sb.AppendLine("</html>");
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "Html Files (*.html)|*.html|All Files (*.*)|*.*" };
            saveDialog.FileName = "Disk Table";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, sb.ToString());
        }

        private void BuildMaps_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            string[] files = openDialog.FileNames;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""generator"" content=""Spectrum Archive Reader"">
        <title>Disk Map Table</title>
        <style>
            .map td { width: 5px; height: 5px; min-width: 5px; min-height: 5px; }
            .good { background-color: #00c000; }
            .gf { background-color: #00ff00; }
            .unp { background-color: #0000c0; }
            .uf { background-color: #0000ff; }
            .bad { background-color: #c00000; }
            .bf { background-color: #ff0000; }
            .zero { background-color: #c0c0c0; }
            .zf { background-color: #e0e0e0; }
            #statsTable tr { text-align: left; margin-top: 30px; }
            #statsTable { margin-top: 30px; }
            #legend div div { width: 15px; height: 15px; float: left; margin-right: 15px; }
            #legend { margin: 30px; }
            #tablediv table { border-spacing: 0px; }
        </style>
    </head>
    <body>
        <div id=""legend"" style=""display: none;"">
            <div><div style=""background-color: #00ff00;""></div>Good in file</div>
            <div><div style=""background-color: #00c000;""></div>Good out of file</div>
            <div><div style=""background-color: #ff0000;""></div>Bad in file</div>
            <div><div style=""background-color: #c00000;""></div>Bad out of file</div>
            <div><div style=""background-color: #e0e0e0;""></div>Zero (good) in file</div>
            <div><div style=""background-color: #c0c0c0;""></div>Zero (good) out of file</div>
            <div><div style=""background-color: #0000ff;""></div>Unprocessed in file</div>
            <div><div style=""background-color: #0000c0;""></div>Unprocessed out of file</div>
        </div>
        <div id=""tablediv""><h1 align=""center"">Rendering HTML ...</h1></div>");
            string indent = "    ";
            int totalSectors = 0;
            int totalGoodSectors = 0;
            int totalBadSectors = 0;
            int totalProcessedSectors = 0;
            int totalFiles = 0;
            int totalDamagedFiles = 0;
            int totalNonZeroSectors = 0;
            int goodDisks = 0;
            int whollyProcessedDisks = 0;
            int sectorsFromWhollyProcessedDisks = 0;
            int goodSectorsFromWhollyProcessedDisks = 0;
            int badSectorsFromWhollyProcessedDisks = 0;
            StringBuilder jsArray = new StringBuilder();
            jsArray.Append("var disks = [");
            StringBuilder values = new StringBuilder();
            values.Append("var values = [");
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage image = new TrDosImage();
                image.LoadAutodetect(files[i]);
                image.ParseCatalogue();
                string disk = image.FileNameOnly.Substring(0, 4);
                values.Append($"[\"{Path.GetFileNameWithoutExtension(image.FileNameOnly)}\",\"{(image.Title == null ? "" : image.Title.Trim('\0', ' '))}\",{image.SizeTracks},{image.GoodSectors},{image.BadSectors},{image.UnprocessedSectors},{image.Files.Cnt},{image.DamagedFiles},{image.NonZeroSectors}],");
                jsArray.Append("\"" + image.GetSectorsAsString() + "\",");
                if (image.GoodSectors == image.SizeSectors && image.SizeTracks >= 160) goodDisks++;
                totalSectors += Math.Max(2560, image.Sectors.Length);
                totalProcessedSectors += image.ProcessedSectors;
                totalGoodSectors += image.GoodSectors;
                totalBadSectors += image.BadSectors;
                totalFiles += image.Files.Cnt;
                totalDamagedFiles += image.DamagedFiles;
                totalNonZeroSectors += image.NonZeroSectors;
                if (image.UnprocessedSectors == 0 && image.SizeTracks >= 160)
                {
                    whollyProcessedDisks++;
                    sectorsFromWhollyProcessedDisks += image.SizeSectors;
                    goodSectorsFromWhollyProcessedDisks += image.GoodSectors;
                    badSectorsFromWhollyProcessedDisks += image.BadSectors;
                }
            }
            jsArray.Length--;
            jsArray.Append("];");
            values.Length--;
            values.Append("];");
            sb.AppendLine(indent + indent + "<table id=\"statsTable\" style=\"display: none;\">");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Total Disks</th><td>{files.Length}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>100%-Good Disks</th><td>{goodDisks}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Assumed Total Sectors</th><td>{totalSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Processed Sectors</th><td>{totalProcessedSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors</th><td>{totalGoodSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors</th><td>{totalBadSectors}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Non-zero Sectors</th><td>{totalNonZeroSectors}</td></tr>");

            sb.AppendLine(indent + indent + indent + $"<tr><th>Wholly Processed Disks</th><td>{whollyProcessedDisks}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Sectors On Wholly Processed Disks</th><td>{sectorsFromWhollyProcessedDisks}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors On Wholly Processed Disks</th><td>{goodSectorsFromWhollyProcessedDisks} ({GP.ToString((double)goodSectorsFromWhollyProcessedDisks / sectorsFromWhollyProcessedDisks * 100, 2)} %)</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors On Wholly Processed Disks</th><td>{badSectorsFromWhollyProcessedDisks} ({GP.ToString((double)badSectorsFromWhollyProcessedDisks / sectorsFromWhollyProcessedDisks * 100, 2)} %)</td></tr>");

            sb.AppendLine(indent + indent + indent + $"<tr><th>Files</th><td>{totalFiles}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Damaged Files</th><td>{totalDamagedFiles}</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors As Share Of Assumed Total</th><td>{GP.ToString((double)totalGoodSectors / totalSectors * 100, 2)} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Sectors As Share Of Processed</th><td>{GP.ToString((double)totalGoodSectors / totalProcessedSectors * 100, 2)} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors As Share Of Total</th><td>{GP.ToString((double)totalBadSectors / totalSectors * 100, 2)} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Bad Sectors As Share Of Processed</th><td>{GP.ToString((double)totalBadSectors / totalProcessedSectors * 100, 2)} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Good Files Share</th><td>{GP.ToString((double)(totalFiles - totalDamagedFiles) / totalFiles * 100, 2)} %</td></tr>");
            sb.AppendLine(indent + indent + indent + $"<tr><th>Damaged Files Share</th><td>{GP.ToString((double)totalDamagedFiles / totalFiles * 100, 2)} %</td></tr>");
            sb.AppendLine(indent + indent + "</table>");
            sb.AppendLine(indent + indent + "<script>");
            sb.AppendLine(indent + indent + indent + jsArray.ToString());
            sb.AppendLine(indent + indent + indent + values.ToString());
            sb.AppendLine(indent + indent + indent + @"var s = """";
            for (var d = 0; d < disks.length; d++)
            {
                var pars = values[d];
                s += ""<div>"" + pars[0] + "" | Title: "" + pars[1] + "" | Size: "" + pars[2] + "" | Good: "" + pars[3] + "" | Bad: "" 
                    + pars[4] + "" | Unprocessed: "" + pars[5] + "" | Files: "" + pars[6] + "" | Damaged: "" + pars[7] + "" | Non-Zero: "" 
                    + pars[8] + ""</div>"";
                s += ""<table class=\""map\"">"";
                var sectors = disks[d];
                var trackCnt = sectors.length / 16;
                if (trackCnt * 16 < sectors.length) trackCnt++;
                for (var sec = 0; sec < 16; sec++)
                {
                    s += ""<tr>"";
                    for (var tr = 0; tr < trackCnt; tr++)
                    {
                        var sectorNum = tr * 16 + sec;
                        if (sectorNum >= sectors.length)
                        {
                            s += ""<td class=\""unp\""></td>"";
                            continue;
                        }
                        switch (sectors[sectorNum])
                        {
                            case 'u':
                                s += ""<td class=\""unp\""></td>"";
                                break;

                            case 'U':
                                s += ""<td class=\""uf\""></td>"";
                                break;

                            case 'g':
                                s += ""<td class=\""good\""></td>"";
                                break;

                            case 'G':
                                s += ""<td class=\""gf\""></td>"";
                                break;

                            case 'b':
                                s += ""<td class=\""bad\""></td>"";
                                break;

                            case 'B':
                                s += ""<td class=\""bf\""></td>"";
                                break;

                            case 'z':
                            	s += ""<td class=\""zero\""></td>"";
                            	break;

                            case 'Z':
                            	s += ""<td class=\""zf\""></td>"";
                            	break;
                        }
                    }
                    s += ""</tr>"";
                }
                s += ""</table>"";
            }
            document.getElementById(""tablediv"").innerHTML = s;
            document.getElementById(""statsTable"").style.display = ""block"";
            document.getElementById(""legend"").style.display = ""block"";
            var tdMouseMove = function(e)
            {
                var elem = document.elementFromPoint(e.clientX, e.clientY);
                if (elem.nodeName != ""TD"") return;
                elem.title = ""Track: "" + elem.cellIndex  + "" | Sector: "" + elem.parentNode.rowIndex;
            }
            var maps = document.querySelectorAll("".map"");
            for (var i = 0; i < maps.length; i++)
            {
                maps[i].onmousemove = tdMouseMove;
            }
        </script>");
            sb.AppendLine(indent + "</body>");
            sb.AppendLine("</html>");
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "Html Files (*.html)|*.html|All Files (*.*)|*.*" };
            saveDialog.FileName = "Maps";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, sb.ToString());
        }

        private byte[] GetMfmSyncArray(byte[] data)
        {
            byte[] r = new byte[data.Length];
            int prevValue = 0;
            for (int i = 0; i < r.Length; i++)
            {
                int mask = 1;
                int b = 0;
                for (int m = 0; m < 8; m++)
                {
                    int valuei = (data[i] & (mask << m)) == 0 ? 0 : 1;
                    if (valuei == 0 && prevValue == 0) b |= mask << m;
                }
                r[i] = (byte)b;
            }
            return r;
        }

        private byte[] GetMfmSyncArray1(byte[] data)
        {
            byte[] r = new byte[data.Length];
            int prevValue = 0;
            for (int i = 0; i < r.Length; i++)
            {
                int mask = 128;
                int b = 0;
                for (int m = 0; m < 8; m++)
                {
                    int valuei = (data[i] & (mask >> m)) == 0 ? 0 : 1;
                    if (valuei == 0 && prevValue == 0) b |= mask << m;
                }
                r[i] = (byte)b;
            }
            return r;
        }

        private void Test_Click(object sender, EventArgs e)
        {
            using (XmlTextWriter xml = new XmlTextWriter(Path.ChangeExtension(Application.ExecutablePath, ".xml"), new UTF8Encoding()))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement("General");
                xml.WriteStartElement("HT");
                xml.WriteAttributeString("localname", "value");
                xml.WriteEndElement();
                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
            return;

            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            TrDosImage image = new TrDosImage();
            int result = image.LoadAutodetect(openDialog.FileName);
            int attemptCount = 50;
            //int track = 65;
            //int sector = 11;

            // D028:
            int track = 121;
            int sector = 15;

            //int sectorNum = track * 16 + sector - 931;  // for D009 DISAB-17.
            int sectorNum = track * 16 + sector;
            int adr = sectorNum * 256;
            DataRate dataRate = DataRate.FD_RATE_250K;
            IntPtr driverHandle = Driver.Open(dataRate);
            Driver.Seek(driverHandle, track);
            IntPtr memoryHandle = Driver.VirtualAlloc(256);
            int[] stats = new int[256 * 8];
            MList<byte[]> blocks = new MList<byte[]>();
            byte[] array = new byte[256];
            byte[] array1 = new byte[256];
            byte[] original = new byte[256];
            Array.Copy(image.Data, adr, original, 0, 256);
            int si;
            int error23Cnt = 0;
            int mismatchCntr;
            int mismatchBitCntr;
            for (int i = 0; i < attemptCount; i++)
            {
                int error = Driver.ReadSector(driverHandle, memoryHandle, track, sector + 1, UpperSideHead.Head0, 1);
                Application.DoEvents();
                if (error == 0)
                {
                    Log.Info?.Out($"Сектор прочитался успешно. Попытка: {i}");
                    continue;
                }
                if (error != 23)
                {
                    Log.Info?.Out($"Ошибка не 23: {error}");
                    continue;
                }
                Marshal.Copy(memoryHandle, array, 0, 256);
                mismatchCntr = 0;
                mismatchBitCntr = 0;
                for (int t = 0; t < 256; t++)
                {
                    if (image.Data[adr + t] != array[t])
                    {
                        mismatchCntr++;
                        int mask = 1;
                        for (int m = 0; m < 8; m++)
                        {
                            if ((image.Data[adr + t] & (mask << m)) != (array[t] & (mask << m))) mismatchBitCntr++;
                        }
                    }
                }
                Log.Info?.Out($"Попытка: {i}. Error: {error}. Mismatched bytes: {mismatchCntr}, bits: {mismatchBitCntr}");
                byte[] newarray = new byte[256];
                array.CopyTo(newarray, 0);
                blocks.Add(newarray);
                error23Cnt++;
                //si = 0;
                //for (int c = 0; c < 256; c++)
                //{
                //    int mask = 1;
                //    for (int m = 0; m < 8; m++)
                //    {
                //        if ((array[c] & (mask << m)) != 0) stats[si]++;
                //        si++;
                //    }
                //}
            }
            Log.Info?.Out($"error23cnt: {error23Cnt}");

            if (error23Cnt == 0)
            {
                Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
                Driver.Close(driverHandle);
                return;
            }
            MList<byte[]> cluster1 = new MList<byte[]>();
            MList<byte[]> cluster2 = new MList<byte[]>();
            cluster1.Add(blocks[0]);
            for (int i = 1; i < blocks.Cnt; i++)
            {
                mismatchCntr = 0;
                for (int t = 0; t < 256; t++)
                {
                    if (blocks[i][t] != blocks[0][t]) mismatchCntr++;
                }
                if (mismatchCntr < 50)
                {
                    cluster1.Add(blocks[i]);
                }
                else
                {
                    cluster2.Add(blocks[i]);
                }
            }
            Log.Info?.Out($"Cluster1: {cluster1.Cnt}. Cluster2: {cluster2.Cnt}");

            // cluster1

            for (int i = 0; i < cluster1.Cnt; i++)
            {
                si = 0;
                for (int c = 0; c < 256; c++)
                {
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((cluster1[i][c] & (mask << m)) != 0) stats[si]++;
                        si++;
                    }
                }
            }
            si = 0;
            for (int i = 0; i < 256; i++)
            {
                byte resByte = 0;
                for (int m = 0; m < 8; m++)
                {
                    int value = stats[si] > cluster1.Cnt / 2 ? 1 : 0;
                    resByte |= (byte)(value << m);
                    si++;
                }
                array[i] = resByte;
            }
            mismatchCntr = 0;
            mismatchBitCntr = 0;
            for (int t = 0; t < 256; t++)
            {
                if (image.Data[adr + t] != array[t])
                {
                    mismatchCntr++;
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((image.Data[adr + t] & (mask << m)) != (array[t] & (mask << m))) mismatchBitCntr++;
                    }
                }
                array1[t] = (byte)(image.Data[adr + t] - array[t]);
            }
            Log.Info?.Out($"Cluster1 mismatched bytes: {mismatchCntr}, bits: {mismatchBitCntr}");

            // cluster2

            Array.Clear(stats, 0, stats.Length);
            for (int i = 0; i < cluster2.Cnt; i++)
            {
                si = 0;
                for (int c = 0; c < 256; c++)
                {
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((cluster2[i][c] & (mask << m)) != 0) stats[si]++;
                        si++;
                    }
                }
            }
            si = 0;
            for (int i = 0; i < 256; i++)
            {
                byte resByte = 0;
                for (int m = 0; m < 8; m++)
                {
                    int value = stats[si] > cluster2.Cnt / 2 ? 1 : 0;
                    resByte |= (byte)(value << m);
                    si++;
                }
                array[i] = resByte;
            }
            mismatchCntr = 0;
            mismatchBitCntr = 0;
            for (int t = 0; t < 256; t++)
            {
                if (image.Data[adr + t] != array[t])
                {
                    mismatchCntr++;
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((image.Data[adr + t] & (mask << m)) != (array[t] & (mask << m))) mismatchBitCntr++;
                    }
                }
                array1[t] = (byte)(image.Data[adr + t] - array[t]);
            }
            Log.Info?.Out($"Cluster2 mismatched bytes: {mismatchCntr}, bits: {mismatchBitCntr}");

            // cluster1 MFM-sync

            Array.Clear(stats, 0, stats.Length);
            for (int i = 0; i < cluster1.Cnt; i++)
            {
                si = 0;
                byte[] mfmSync = GetMfmSyncArray(cluster1[i]);
                for (int c = 0; c < 256; c++)
                {
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((mfmSync[c] & (mask << m)) != 0) stats[si]++;
                        si++;
                    }
                }
            }
            si = 0;
            for (int i = 0; i < 256; i++)
            {
                byte resByte = 0;
                for (int m = 0; m < 8; m++)
                {
                    int value = stats[si] > cluster1.Cnt / 2 ? 1 : 0;
                    resByte |= (byte)(value << m);
                    si++;
                }
                array[i] = resByte;
            }
            mismatchCntr = 0;
            mismatchBitCntr = 0;
            for (int t = 0; t < 256; t++)
            {
                if (image.Data[adr + t] != array[t])
                {
                    mismatchCntr++;
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((image.Data[adr + t] & (mask << m)) != (array[t] & (mask << m))) mismatchBitCntr++;
                    }
                }
                array1[t] = (byte)(image.Data[adr + t] - array[t]);
            }
            Log.Info?.Out($"Cluster1 mismatched bytes: {mismatchCntr}, bits: {mismatchBitCntr}");

            // cluster2 MFM-Sync

            Array.Clear(stats, 0, stats.Length);
            for (int i = 0; i < cluster2.Cnt; i++)
            {
                si = 0;
                byte[] mfmSync = GetMfmSyncArray(cluster2[i]);
                for (int c = 0; c < 256; c++)
                {
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((mfmSync[c] & (mask << m)) != 0) stats[si]++;
                        si++;
                    }
                }
            }
            si = 0;
            for (int i = 0; i < 256; i++)
            {
                byte resByte = 0;
                for (int m = 0; m < 8; m++)
                {
                    int value = stats[si] > cluster2.Cnt / 2 ? 1 : 0;
                    resByte |= (byte)(value << m);
                    si++;
                }
                array[i] = resByte;
            }
            mismatchCntr = 0;
            mismatchBitCntr = 0;
            for (int t = 0; t < 256; t++)
            {
                if (image.Data[adr + t] != array[t])
                {
                    mismatchCntr++;
                    int mask = 1;
                    for (int m = 0; m < 8; m++)
                    {
                        if ((image.Data[adr + t] & (mask << m)) != (array[t] & (mask << m))) mismatchBitCntr++;
                    }
                }
                array1[t] = (byte)(image.Data[adr + t] - array[t]);
            }
            Log.Info?.Out($"Cluster2 mismatched bytes: {mismatchCntr}, bits: {mismatchBitCntr}");

            //si = 0;
            //for (int i = 0; i < 256; i++)
            //{
            //    byte resByte = 0;
            //    for (int m = 0; m < 8; m++)
            //    {
            //        int value = stats[si] > error23Cnt / 2 ? 1 : 0;
            //        resByte |= (byte)(value << m);
            //        si++;
            //    }
            //    array[i] = resByte;
            //}
            //mismatchCntr = 0;
            //for (int i = 0; i < 256; i++)
            //{
            //    if (image.Data[adr + i] != array[i]) mismatchCntr++;
            //}
            //Log.Info?.Out($"Несовпадающие байты: {mismatchCntr}");
            Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            Driver.Close(driverHandle);
            return;

            //OpenFileDialog openDialog = new OpenFileDialog();
            //if (openDialog.ShowDialog() != DialogResult.OK) return;
            //TrDosImage image = new TrDosImage();
            //int result = image.LoadAutodetect(openDialog.FileName, new TrackFormat(TrackFormatName.TrDosSequential));
            //int track = 121;
            //int sector = 15;
            //int sectorNum = 121 * 16 + 15;
            //byte[] array = new byte[256];
            //Array.Copy(image.Data, sectorNum * 256, array, 0, 256);
            //StringBuilder sb = new StringBuilder(array.Length * 8 * 2);
            //int prevValue = 0;
            //for (int i = 0, j = 1, c = 0; j < array.Length; i++, j++)
            //{
            //    int mask = 1;
            //    for (int m = 0; m < 8; m++)
            //    {
            //        int valuei = (array[i] & (mask << m)) == 0 ? 0 : 1;
            //        int valuej = (array[j] & (mask << m)) == 0 ? 0 : 1;
            //        sb.Append(valuei == 0 && prevValue == 0 ? '+' : '-');
            //        sb.Append(valuei == 0 ? '0' : '1');
            //        prevValue = valuei;
            //    }
            //}
            //Clipboard.SetText(sb.ToString());
            //return;

            //DataRate dataRate = DataRate.FD_RATE_250K;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //int track = 0;
            //Driver.Seek(driverHandle, track);
            //IntPtr memoryHandle = Driver.VirtualAlloc(8192);
            //int error = Driver.ReadSector(driverHandle, memoryHandle, 0, 9, UpperSideHead.Head0, 1);
            //Log.Info?.Out($"Error: {error}");
            //byte[] dest = new byte[8192];
            //Marshal.Copy(memoryHandle, dest, 0, 8192);
            //for (int i = 0; i < dest.Length; i++)
            //{
            //    if (dest[i] != 0)
            //    {
            //        Log.Info?.Out($"Данные не нулевые.");
            //        break;
            //    }
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //return;

            //DataRate dataRate = DataRate.FD_RATE_250K;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //int track = 83;
            //int sector = 6;     //83-5(6) | 77-5,6,7 | 75-4,5,6 | 97-6,7,8 
            //IntPtr memoryHandle = Driver.VirtualAlloc(256);
            //Random random = new Random();
            //for (int i = 0; i < 50; i++)
            //{
            //    int diff = 4;
            //    Driver.Seek(driverHandle, track + random.Next(diff) - diff / 2);
            //    Thread.Sleep(random.Next(200));
            //    //Driver.Seek(driverHandle, track + 7);
            //    Driver.Seek(driverHandle, track);
            //    int error = Driver.ReadSector(driverHandle, memoryHandle, track, sector, UpperSideHead.Head0, 1);
            //    if (error == 0)
            //    {
            //        Log.I?.InfoI("Sector read successfully.");
            //        break;
            //    }
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //return;


            //DataRate dataRate = DataRate.FD_RATE_250K;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //int track = 87;
            //int sector = 6;
            //IntPtr memoryHandle = Driver.VirtualAlloc(256);
            //for (int i = 0; i < 100; i++)
            //{
            //    Driver.Seek(driverHandle, track);
            //    int error = Driver.ReadSector(driverHandle, memoryHandle, track, sector, UpperSideHead.Head0, 1);
            //    if (error == 0)
            //    {
            //        Log.I?.InfoI("Sector read successfully.");
            //        break;
            //    }
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //return;


            //DataRate dataRate = DataRate.FD_RATE_250K;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //Log.Info?.Out($"result: {Driver.Seek(driverHandle, 171)}");
            //int track = 10;
            //Driver.Seek(driverHandle, track);
            //tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            //{
            //    flags = Driver.FD_OPTION_MFM,
            //    head = (byte)(track & 1)
            //};
            //tagFD_CMD_RESULT cmdResult = new tagFD_CMD_RESULT();
            //Driver.ReadId(driverHandle, pars, out cmdResult);
            //Log.Info?.Out($"result: {cmdResult.cyl}");
            //Driver.Close(driverHandle);
            //return;


            //DataRate dataRate = DataRate.FD_RATE_250K;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //IntPtr memoryHandle = Driver.VirtualAlloc(65536);
            //tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS() { flags = Driver.FD_OPTION_MFM };
            //tagFD_CMD_RESULT cmdResult = new tagFD_CMD_RESULT();
            //int trackFrom = 10;
            //int trackTo = 30;
            //Driver.Seek(driverHandle, trackFrom);
            //Timer totalTime = Timer.StartNew();
            //Timer timer = Timer.StartNew();
            //tagFD_CMD_RESULT cmdF = new tagFD_CMD_RESULT();
            //for (int track = trackFrom; track < trackTo; track++)
            //{
            //    pars.head = (byte)(track & 1);
            //    Driver.Seek(driverHandle, track);
            //    Driver.ReadId(driverHandle, pars, out cmdResult);
            //    Log.T?.OutI($"Track: {track} | Found ID: {cmdResult.sector} | TotalTime: {GP.ToString(totalTime.ElapsedMs, 2)} | TimeSpan: {GP.ToString(timer.ElapsedMs, 2)}");
            //    timer.Restart();
            //    cmdF = cmdResult;
            //    while (true)
            //    {
            //        Driver.ReadId(driverHandle, pars, out cmdResult);
            //        Log.T?.OutI($"Track: {track} | Found ID: {cmdResult.sector} | TotalTime: {GP.ToString(totalTime.ElapsedMs, 2)} | TimeSpan: {GP.ToString(timer.ElapsedMs, 2)}");
            //        timer.Restart();
            //        if (cmdResult.sector == cmdF.sector) break;
            //    }
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //return;


            //DataRate dataRate = DataRate.FD_RATE_250K;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //IntPtr memoryHandle = Driver.VirtualAlloc(65536);
            ////bool r = Driver.CheckDisk(driverHandle);
            ////Log.T?.OutI($"Result: {r}");
            //Driver.Seek(driverHandle, 10);
            //int error = Driver.ReadTrack(driverHandle, memoryHandle, 10, 1, UpperSideHead.Head0);
            //Log.T?.OutI($"error: {WinErrors.GetSystemMessage(error)}");
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //return;


            //DataRate dataRate = (DataRate)C_DataRate.SelectedIndex;
            //IntPtr driverHandle = Driver.Open(dataRate);
            //if ((int)driverHandle == Driver.INVALID_HANDLE_VALUE) return;
            //IntPtr memoryHandle = Driver.VirtualAlloc(256);
            //Driver.Seek(driverHandle, 10);
            //for (int i = 0; i < 30; i++)
            //{
            //    Driver.ReadSectorF(driverHandle, memoryHandle, 5, 16, 0, 0, i, 255);
            //    Driver.ReadSectorF(driverHandle, memoryHandle, 5, 1, 0, 0, i, 255);
            //}
            //for (int i = 0; i < 30; i++)
            //{
            //    Driver.ReadSectorF(driverHandle, memoryHandle, 5, 16, 0, 0, i, 128);
            //    Driver.ReadSectorF(driverHandle, memoryHandle, 5, 1, 0, 0, i, 128);
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //Log.I?.InfoI("Completed.");
            //return;

            //DataRate dataRate = (DataRate)C_DataRate.SelectedIndex;
            //Log.T?.OutI($"DataRate: {dataRate} | High Resolution Timer Available: {Stopwatch.IsHighResolution}");
            //DiskReader diskReader = new DiskReader();
            //IntPtr driverHandle = Driver.Open(dataRate);
            //diskReader.driverHandle = driverHandle;
            //if ((int)driverHandle == Driver.INVALID_HANDLE_VALUE) return;
            //IntPtr memoryHandle = Driver.VirtualAlloc(256);
            //if (memoryHandle == IntPtr.Zero) return;
            //Driver.Seek(driverHandle, 10);
            //TrackFormat trackFormat = null;
            //for (int i = 0; i < 2; i++)
            //{
            //    Log.T?.OutI("Scanning track");
            //    trackFormat = diskReader.ScanFormat(10);
            //    for (int j = 0; j < trackFormat.Layout.Cnt; j++)
            //    {
            //        Log.T?.OutI($"Cyl: {trackFormat.Layout.Data[j].Cylinder} Head: {trackFormat.Layout.Data[j].Head} Sector: {trackFormat.Layout.Data[j].SectorNumber} Size: {trackFormat.Layout.Data[j].Size} Time: {GP.ToString(trackFormat.Layout.Data[j].TimeMs, 2)} ");
            //    }
            //}
            //if (trackFormat != null)
            //{
            //    for (int i = 0; i < trackFormat.Layout.Cnt; i++)
            //    {
            //        Driver.ReadSectorF(driverHandle, memoryHandle, trackFormat.Layout.Data[i].Cylinder, trackFormat.Layout.Data[i].SectorNumber, 0, trackFormat.Layout.Data[i].Head, 0x0a);
            //    }
            //}
            //int time;
            //if (Driver.GetTrackTime(driverHandle, out time))
            //{
            //    Log.T?.OutI($"Track Time: {time} ms");
            //}
            //else
            //{
            //    Log.T?.OutI($"Function GetTrackTime returned false.");
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            //Driver.Close(driverHandle);
            //Log.I?.InfoI("Completed.");
            //return;

            //IntPtr handle = Driver.CreateFile("\\\\.\\a:", 0, 0, IntPtr.Zero, Driver.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            //FDC\\GENERIC_FLOPPY_DRIVE
            //IntPtr handle = Driver.CreateFile("PNP0700\0*PNP0700\0\0", 0, 0, IntPtr.Zero, Driver.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            //if (handle != (IntPtr)Driver.INVALID_HANDLE_VALUE)
            //{
            //    Driver.CloseHandle(handle);
            //}
            //return;


            //IntPtr h = Driver.Open(settings.DataRate);

            //IntPtr memoryHandle = Driver.VirtualAlloc(IntPtr.Zero, (UIntPtr)65536, Driver.AllocationType.Commit, Driver.MemoryProtection.ReadWrite);
            //Stopwatch stopwatch = new Stopwatch();
            //Driver.ReadSector(h, memoryHandle, 0, 1, UpperSideHead.UpperSideAs0);
            //stopwatch.Start();
            ////Thread.Sleep(new TimeSpan((long)(TimeSpan.TicksPerMillisecond * 1.5)));
            //Driver.ReadSector(h, memoryHandle, 0, 2, UpperSideHead.UpperSideAs0);
            //Log.T?.OutI($"time: {GP.ToString((double)stopwatch.Elapsed.Ticks / TimeSpan.TicksPerMillisecond, 4)}");
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);

            //int time;
            //Driver.GetTrackTime(h, out time);
            //Log.T?.OutI($"time: {time}");

            //IntPtr memoryHandle = Driver.VirtualAlloc(IntPtr.Zero, (UIntPtr)65536, Driver.AllocationType.Commit, Driver.MemoryProtection.ReadWrite);
            //int track = 10;
            //Driver.Seek(h, track, DiskFormat.UpperSideAs0);
            //Driver.ReadSector(h, memoryHandle, track, 15, DiskFormat.UpperSideAs0, true);
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            //{
            //    flags = Driver.FD_OPTION_MFM,
            //    head = 0
            //};
            //tagFD_CMD_RESULT cmdResult;
            //Driver.ReadId(h, pars, out cmdResult);
            //stopwatch.Stop();
            //double timeMs = (double)stopwatch.Elapsed.Ticks / TimeSpan.TicksPerMillisecond;
            //Log.T?.OutI($"Time: {GP.ToString(timeMs, 4)}");
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);

            //Driver.Seek(h, 0, DiskFormat.UpperSideAs0);
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //Driver.Seek(h, 100, DiskFormat.UpperSideAs0);
            //stopwatch.Stop();
            //Log.T?.OutI($"time: {stopwatch.ElapsedMilliseconds}");

            //Driver.SetHeadSettleTime(h, 20);
            //Stopwatch stopwatch = new Stopwatch();
            //Driver.Seek(h, 0, DiskFormat.UpperSideAs0);
            //for (int i = 1; i < 10; i++)
            //{
            //    stopwatch.Reset();
            //    stopwatch.Start();
            //    Driver.Seek(h, i * 2, DiskFormat.UpperSideAs0);
            //    stopwatch.Stop();
            //    Log.T?.OutI($"time: {stopwatch.ElapsedMilliseconds}");
            //    Thread.Sleep(1000);
            //}

            //IntPtr memoryHandle = Driver.VirtualAlloc(IntPtr.Zero, (UIntPtr)65536, Driver.AllocationType.Commit, Driver.MemoryProtection.ReadWrite);
            //int track = 10;
            //Driver.Seek(h, track, DiskFormat.UpperSideAs0);
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //int sector = 0;
            //for (int i = 0; i < 20; i++)
            //{
            //    Driver.ReadSector(h, memoryHandle, track, sector, DiskFormat.UpperSideAs0, true);
            //    double timeMs = (double)stopwatch.Elapsed.Ticks / TimeSpan.TicksPerMillisecond;
            //    Log.T?.OutI($"sector={sector + 1} | TimeSpan: {GP.ToString(timeMs, 2)}");
            //    stopwatch.Reset();
            //    stopwatch.Start();
            //    sector++;
            //    if (sector > 15) sector = 0;
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);

            //IntPtr memoryHandle = Driver.VirtualAlloc(IntPtr.Zero, (UIntPtr)65536, Driver.AllocationType.Commit, Driver.MemoryProtection.ReadWrite);
            //Driver.Seek(h, 10, DiskFormat.UpperSideAs0);
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //for (int i = 0; i < 20; i++)
            //{
            //    tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            //    {
            //        flags = Driver.FD_OPTION_MFM,
            //        head = 1
            //    };
            //    tagFD_CMD_RESULT cmdResult;
            //    Driver.ReadId(h, pars, out cmdResult);
            //    double timeMs = (double)stopwatch.Elapsed.Ticks / TimeSpan.TicksPerMillisecond;
            //    Log.T?.OutI($"st0={cmdResult.st0} st1={cmdResult.st1} st2={cmdResult.st2} cyl={cmdResult.cyl} head={cmdResult.head} sector={cmdResult.sector} size={cmdResult.size} | TimeSpan: {GP.ToString(timeMs, 2)}");
            //    stopwatch.Reset();
            //    stopwatch.Start();
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);

            //IntPtr memoryHandle = Driver.VirtualAlloc(IntPtr.Zero, (UIntPtr)65536, Driver.AllocationType.Commit, Driver.MemoryProtection.ReadWrite);
            //Driver.Seek(h, 80, DiskFormat.UpperSideAs0);
            //for (int i = 0; i < 20; i++)
            //{
            //    tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            //    {
            //        flags = Driver.FD_OPTION_MFM,
            //        head = 0
            //    };
            //    tagFD_CMD_RESULT cmdResult;
            //    Driver.ReadId(h, pars, out cmdResult);
            //    Log.T?.OutI($"st0={cmdResult.st0} st1={cmdResult.st1} st2={cmdResult.st2} cyl={cmdResult.cyl} head={cmdResult.head} sector={cmdResult.sector} size={cmdResult.size}");
            //    Driver.ReadSectorP(h, memoryHandle, 80, cmdResult.sector - 1, DiskFormat.UpperSideAs0, false);
            //}
            //Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);

            //bool sr = Driver.SetHeadSettleTime(h, 50);
            //int error = Marshal.GetLastWin32Error();
            //Log.T?.OutI($"sr={sr} | Error: {error} | Message: {WinErrors.GetSystemMessage(error)}");
            //
            //for (int track = 50; track < 70; track++)
            //{
            //    Driver.Seek(h, track, DiskFormat.UpperSideAs0);
            //    for (int i = 0; i < 16; i++)
            //    {
            //        tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            //        {
            //            flags = Driver.FD_OPTION_MFM,
            //            head = (byte)(track & 1)
            //        };
            //        tagFD_CMD_RESULT cmdResult;
            //        Driver.ReadId(h, pars, out cmdResult);
            //        Log.T?.OutI($"{i} {pars.head}: st0={cmdResult.st0} st1={cmdResult.st1} st2={cmdResult.st2} cyl={cmdResult.cyl} head={cmdResult.head} sector={cmdResult.sector} size={cmdResult.size}");
            //    }
            //}

            //Driver.Seek(h, 80, DiskFormat.UpperSideAs0);
            //for (int i = 0; i < 20; i++)
            //{
            //    tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            //    {
            //        flags = Driver.FD_OPTION_MFM,
            //        head = 0
            //    };
            //    tagFD_CMD_RESULT cmdResult;
            //    Driver.ReadId(h, pars, out cmdResult);
            //    Log.T?.OutI($"st0={cmdResult.st0} st1={cmdResult.st1} st2={cmdResult.st2} cyl={cmdResult.cyl} head={cmdResult.head} sector={cmdResult.sector} size={cmdResult.size}");
            //}

            //tagFD_INTERRUPT_STATUS interruptStatus;
            //bool r = Driver.Recalibrate(h, out interruptStatus);
            //bool r = Driver.SetHeadSettleTime(h, 5);
            //Log.T?.OutI("result: " + r);
            //byte result;
            //Driver.GetPartId(h, out result);
            //Log.T?.OutI(result.ToString());
            //int time;
            //bool r = Driver.GetTrackTime(h, out time);
            //tagFD_CMD_RESULT result;
            //Driver.GetResult(h, out result);
            //Log.T?.OutI(result.st0.ToString());
            //Driver.MotorOn(h);

            //tagFD_SENSE_PARAMS senseParams = new tagFD_SENSE_PARAMS() { head = 1 };
            //tagFD_DRIVE_STATUS driveStatus;
            //bool r = Driver.SenseDriveStatus(h, senseParams, out driveStatus);
            //Log.T?.OutI($"result: {r} | Param: {driveStatus.st3} | WP={driveStatus.st3 & 0x40} | TR00={driveStatus.st3 & 0x10}");

            //tagFD_DUMPREG_RESULT dump;
            //bool r = Driver.DumpRegister(h, out dump);
            //Log.T?.OutI(r.ToString());

            //int time;
            //bool r = Driver.GetTrackTime(h, out time);
            //Log.T?.OutI("time=" + time + " result: " + r);
            //int error = Marshal.GetLastWin32Error();
            //Log.T?.OutI($"sr={r} | Error: {error} | Message: {WinErrors.GetSystemMessage(error)}");

            //for (int i = 0; i < 160; i++)
            //{
            //    int res = Driver.ScanTrack(h, i, DiskFormat.UpperSideAs0);
            //    Log.T?.OutI($"Track: {i} | Error: {res}");
            //}
            //IntPtr memoryHandle = Driver.VirtualAlloc(IntPtr.Zero, (UIntPtr)65536, Driver.AllocationType.Commit, Driver.MemoryProtection.ReadWrite);
            //int r = Driver.ReadTrack(h, memoryHandle, 20, 0, DiskFormat.UpperSideAs0);
            //Driver.Close(h);
            return;
            //if (!ReadParameters()) return;
            //EnableControls(true);
            //Log.T?.OutI($"Чтение диска. Disk Format: {settings.DiskFormat}. Side: {settings.ReadSide}. Rate: {settings.DataRate}");
            //BackgroundWorker worker = new BackgroundWorker();
            //aborted = false;
            //worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            //{
            //    try
            //    {
            //        IntPtr handle = Driver.Open(settings.DataRate);
            //        int successfullyRead = 0;
            //        for (int i = 0; i < 10; i++)
            //        {
            //            if (aborted) break;
            //            ReadResult result = Driver.ReadSectorArray(handle, i, settings.DiskFormat);
            //            if (result.Error == 0) successfullyRead += 16;
            //        }
            //        Driver.Close(handle);
            //        Log.T?.OutI($"Успешно прочитанных секторов: {successfullyRead}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.E?.ErrorI($"Исключение при чтении диска: {ex.Message} | StackTrace: {ex.StackTrace}");
            //    }
            //};
            //worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            //{
            //    EnableControls(false);
            //};
            //worker.RunWorkerAsync();
            //return;

            //// Вычисление самых длинных последовательностей B и N на дисках.
            //OpenFileDialog openDialog = new OpenFileDialog() { Multiselect = true };
            //if (openDialog.ShowDialog() != DialogResult.OK) return;
            //string[] fileNames = openDialog.FileNames;
            //StringBuilder sb = new StringBuilder();
            //int longestBOverall = 0;
            //int longestNOverall = 0;
            //for (int i = 0; i < fileNames.Length; i++)
            //{
            //    DiskImage image = DiskImage.Load(fileNames[i], 0);
            //    byte[] array = new byte[image.GoodSectors * 256];
            //    int curSec = 0;
            //    for (int j = 0; j < image.Sectors.Length; j++)
            //    {
            //        if (image.Sectors[j] != SectorProcessResult.Good) continue;
            //        Array.Copy(image.Data, j * 256, array, curSec * 256, 256);
            //        curSec++;
            //    }
            //    int maxNLen = 0;
            //    int curNLen = 0;
            //    for (int j = 0; j < array.Length; j++)
            //    {
            //        if (array[j] == 'N')
            //        {
            //            curNLen++;
            //            if (curNLen > maxNLen) maxNLen = curNLen;
            //        }
            //        else
            //        {
            //            curNLen = 0;
            //        }
            //    }
            //    int maxBLen = 0;
            //    int curBLen = 0;
            //    for (int j = 0; j < array.Length; j++)
            //    {
            //        if (array[j] == 'N')
            //        {
            //            curBLen++;
            //            if (curBLen > maxBLen) maxBLen = curBLen;
            //        }
            //        else
            //        {
            //            curBLen = 0;
            //        }
            //    }
            //    if (maxNLen > longestNOverall) longestNOverall = maxNLen;
            //    if (maxBLen > longestBOverall) longestBOverall = maxBLen;
            //    sb.AppendLine($"{Path.GetFileNameWithoutExtension(fileNames[i])}: B={maxBLen} | N={maxNLen}");
            //}
            //sb.AppendLine();
            //sb.AppendLine($"Longest B series: {longestBOverall} | Longest N series: {longestNOverall}");
            //Clipboard.SetText(sb.ToString());
            //return;


            // Битовый сдвиг
            //byte[] sector = new byte[256];
            //byte[] output = new byte[256];
            //int sectorNumber = 7;
            //Array.Copy(image.Data, sectorNumber * 256, sector, 0, 256);
            //StringBuilder sb = new StringBuilder();
            //for (int bits = 0; bits < 8; bits++)
            //{
            //    uint mask = 255U << (8 - bits);
            //    for (int i = 0; i < 255; i++)
            //    {
            //        uint b = (uint)sector[i] << bits;
            //        output[i] = (byte)(b | ((sector[i + 1] & mask) >> (8 - bits)));
            //    }
            //    for (int adr = 0; adr < 256;)
            //    {
            //        for (int u = 0; u < 16; u++, adr++)
            //        {
            //            char c = output[adr] == 0 ? 'Z' : (char)output[adr];
            //            sb.Append(c);
            //            //uint mask1 = 128;
            //            //for (int y = 0; y < 8; y++)
            //            //{
            //            //    sb.Append((output[adr] & mask1) == 0 ? '0' : '1');
            //            //    mask1 >>= 1;
            //            //}
            //        }
            //        sb.AppendLine();
            //    }
            //    sb.AppendLine();
            //    sb.AppendLine();
            //}
            //Clipboard.SetText(sb.ToString());
            //return;

            // Сравнение файлов:
            //OpenFileDialog openDialog = new OpenFileDialog();
            //if (openDialog.ShowDialog() != DialogResult.OK) return;
            //byte[] file0 = File.ReadAllBytes(openDialog.FileName);
            //if (openDialog.ShowDialog() != DialogResult.OK) return;
            //byte[] file1 = File.ReadAllBytes(openDialog.FileName);
            //for (int i = 0, last = Math.Min(file0.Length, file1.Length); i < last; i++)
            //{
            //    if (file0[i] != file1[i])
            //    {
            //        Log.T?.OutI($"Разные байты по адресу: {i}");
            //    }
            //}
            //return;


            //            int len = "23:07:04.698: ERROR: Form1.C_ReadDisk_Click: Не удалось прочитать трек: ".Length;
            //            string messages = @"00:37:52.183: ERROR: Form1.C_ReadDisk_Click: Не удалось прочитать трек: 0
            //00:37:57.312: ERROR: Form1.C_ReadDisk_Click: Не удалось прочитать трек: 1
            //00:38:24.611: ERROR: Form1.C_ReadDisk_Click: Не удалось прочитать трек: 25
            //00:38:34.242: ERROR: Form1.C_ReadDisk_Click: Не удалось прочитать трек: 26";
            //            string[] lines = messages.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //            int lastTrack = 0;
            //            for (int i = 0; i < lines.Length; i++)
            //            {
            //                int track = Int32.Parse(lines[i].Substring(len));
            //                lastTrack = track;
            //                for (int j = track * 16 * 256, last = j + 16 * 256; j < last; j++)
            //                {
            //                    image.Data[j] = (byte)'B';
            //                }
            //                for (int j = track * 16, last = j + 16; j < last; j++)
            //                {
            //                    image.Sectors[j] = 2;
            //                }
            //            }
            //            image.SetSize(lastTrack * 16);
        }

        private void SearchInFiles_Click(object sender, EventArgs e)
        {
            string[] values = new string[] { "1994", "1995", "1996", "1997" };
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            string[] files = openDialog.FileNames;
            MList<DiskString> result = new MList<DiskString>();
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage disk = new TrDosImage();
                disk.LoadAutodetect(files[i]);
                disk.ParseCatalogue();
                for (int j = 0; j < values.Length; j++)
                {
                    MList<DiskString> strs = disk.FindString(values[j], 40, 60);
                    result.AddRange(strs);
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""generator"" content=""Spectrum Archive Reader"">
        <title>Spectrum Signatures</title>
        <style>
			.r { color: red; }
        </style>
    </head>
    <body>
        <table>
            <tr><th>Disk</th><th>File</th><th>Ext</th><th>Size</th><th>Track</th><th>Sector</th><th>Offset</th><th>Xor</th><th>String</th></tr>");
            for (int i = 0; i < result.Cnt; i++)
            {
                sb.AppendLine("            " + result[i].ToHtmlTableRow());
            }
            sb.AppendLine(@"        </table>
    </body>
</html>");
            SaveFileDialog saveDialog = new SaveFileDialog();
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, sb.ToString());
        }

        private void SearchBytesInFiles_Click(object sender, EventArgs e)
        {
            //byte[] bytes = new byte[] { 0x21, 0x37, 0xf2, 0x3d, 0xca, 0x63 };  // Disk UN1, Sector 64-15.
            //byte[] bytes = new byte[] { 0x80, 0x00, 0x01, 0x1f, 0xc0, 0x00 };  // Disk UN1, Sector 35-15.
            //byte[] bytes = new byte[] { 0x2c, 0xb0, 0x22, 0x32, 0x22, 0x3b };  // Disk UN2, Sector 4-9.
            //byte[] bytes = new byte[] { 0x64, 0x32, 0xd5, 0x46, 0xf5, 0x3e };  // Disk UN3, Sector 18-1 (36-1).
            //byte[] bytes = new byte[] { 0x32, 0x35, 0x30, 0x35, 0x30, 0x30 };  // Disk UN3, Sector 50-8 DS.
            //byte[] bytes = new byte[] { 0x21, 0x8b, 0x9e, 0xdd, 0x7e, 0x03 };  // Disk UN4, Sector 3-13.
            //byte[] bytes = new byte[] { 0x4b, 0xae, 0x48, 0x02, 0xb3, 0x45 };  // Disk UN4, Sector 9-11.
            //byte[] bytes = new byte[] { 0x87, 0x01, 0x08, 0x8d, 0x10, 0x37 };  // Disk UN2, Sector 7-6.
            //byte[] bytes = new byte[] { 0xac, 0x25, 0x88, 0x54, 0xa4, 0xa0 };  // Disk UN2, Sector 18-3.
            //byte[] bytes = new byte[] { 0xcd, 0x30, 0x75 };  // CALL #7530
            byte[] bytes = new byte[] { 0x0a, 0x08, 0xbf, 0x0a, 0x04 };  // TASM4.0: ORG
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            string[] files = openDialog.FileNames;
            MList<DiskString> result = new MList<DiskString>();
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage disk = new TrDosImage();
                disk.LoadAutodetect(files[i]);
                disk.ParseCatalogue();
                for (int j = 0; j < bytes.Length; j++)
                {
                    MList<DiskString> strs = disk.FindBytes(bytes);
                    result.AddRange(strs);
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""generator"" content=""Spectrum Archive Reader"">
        <title>Spectrum Signatures</title>
        <style>
			.r { color: red; }
        </style>
    </head>
    <body>
        <table>
            <tr><th>Disk</th><th>File</th><th>Ext</th><th>Size</th><th>Track</th><th>Sector</th><th>Offset</th><th>Xor</th><th>String</th></tr>");
            for (int i = 0; i < result.Cnt; i++)
            {
                sb.AppendLine("            " + result[i].ToHtmlTableRow());
            }
            sb.AppendLine(@"        </table>
    </body>
</html>");
            SaveFileDialog saveDialog = new SaveFileDialog();
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, sb.ToString());
        }

        private void FixTrd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog() { Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            TrDosImage image = new TrDosImage();
            image.LoadAutodetect(openDialog.FileName);
            SaveFileDialog saveDialog = new SaveFileDialog();
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllBytes(saveDialog.FileName, image.ToTrd(32, false));
        }

        private void BuildDuplicateMaps_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            string[] files = openDialog.FileNames;
            StringBuilder jsArray = new StringBuilder();
            jsArray.Append("var disks = [");
            StringBuilder values = new StringBuilder();
            values.Append("var values = [");
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage image = new TrDosImage();
                image.LoadAutodetect(files[i]);
                image.ParseCatalogue();
                string disk = image.FileNameOnly.Substring(0, 4);
                values.Append($"[\"{Path.GetFileNameWithoutExtension(image.FileNameOnly)}\",\"{(image.Title == null ? "" : image.Title.Trim('\0', ' '))}\",{image.SizeTracks},{image.GoodSectors},{image.BadSectors},{image.UnprocessedSectors},{image.Files.Cnt},{image.DamagedFiles},{image.NonZeroSectors}],");
                jsArray.Append("\"" + image.BuildAdjacentSectorsDuplicateSectorsMap(16) + "\",");
            }
            jsArray.Length--;
            jsArray.Append("];");
            values.Length--;
            values.Append("];");
            MList<HtmlMapPars> pars = new MList<HtmlMapPars>()
            {
                new HtmlMapPars('o', "#00c000", "Normal"),
                new HtmlMapPars('x', "#c00000", "Duplicate")
            };
            string html = HtmlMap.BuildMap("Duplicate Sectors", jsArray.ToString(), values.ToString(), pars);
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "Html Files (*.html)|*.html|All Files (*.*)|*.*" };
            saveDialog.FileName = "Duplicate Sectors Maps";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, html);
        }

        private void BuildSectorContentMaps_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "TRD Files (*.trd)|*.trd|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            string[] files = openDialog.FileNames;
            StringBuilder jsArray = new StringBuilder();
            jsArray.Append("var disks = [");
            StringBuilder values = new StringBuilder();
            values.Append("var values = [");
            for (int i = 0; i < files.Length; i++)
            {
                TrDosImage image = new TrDosImage();
                image.LoadAutodetect(files[i]);
                image.ParseCatalogue();
                string disk = image.FileNameOnly.Substring(0, 4);
                values.Append($"[\"{Path.GetFileNameWithoutExtension(image.FileNameOnly)}\",\"{(image.Title == null ? "" : image.Title.Trim('\0', ' '))}\",{image.SizeTracks},{image.GoodSectors},{image.BadSectors},{image.UnprocessedSectors},{image.Files.Cnt},{image.DamagedFiles},{image.NonZeroSectors}],");
                jsArray.Append("\"" + image.BuildSectorContentMap() + "\",");
            }
            jsArray.Length--;
            jsArray.Append("];");
            values.Length--;
            values.Append("];");
            MList<HtmlMapPars> pars = new MList<HtmlMapPars>()
            {
                new HtmlMapPars('t', "#ca2c92", "Text"),
                new HtmlMapPars('c', "#c00000", "Code"),
                new HtmlMapPars('f', "yellow", "Low Entropy: Graphics / Music"),
                new HtmlMapPars('a', "#00c000", "Source code: TASM4.0"),
                new HtmlMapPars('e', "#0000ff", "Source code: GENS / Untokenized"),
                new HtmlMapPars('z', "#c0c0c0", "Zero"),
                new HtmlMapPars('o', "#000000", "Unrecognized / Compressed / Encrypted"),
                new HtmlMapPars('b', "#ffffff", "Bad / Unprocessed Sector"),
            };
            string html = HtmlMap.BuildMap("Sector Content Maps", jsArray.ToString(), values.ToString(), pars);
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "Html Files (*.html)|*.html|All Files (*.*)|*.*" };
            saveDialog.FileName = "Sector Content Maps";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, html);
        }

        private void GetVersion_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Driver Version: {Driver.GetVersionStr()}");
            tagFD_FDC_INFO fdcInfo;
            IntPtr h = Driver.Open();
            if ((int)h == Driver.INVALID_HANDLE_VALUE) return;
            Driver.GetFdcInfo(h, out fdcInfo);
            Driver.Close(h);
            sb.AppendLine($"Controller Type: {fdcInfo.ControllerType.ToString().Replace("FDC_TYPE_", "")}");
            sb.AppendLine($"Speeds Available: {fdcInfo.SpeedsAvailable.ToString().Replace("FDC_SPEED_", "")}");
            MessageBox.Show(sb.ToString());
        }

        private void MeasureRotationSpeed(object sender, EventArgs e)
        {
            //if (!ReadParameters()) return;
            MainForm_OperationStarted(sender, e);
            //Log.I?.Out($"Измерение скорости вращения диска. Disk Format: {settings.DiskFormat}. Side: {settings.ReadSide}. DataRate: {settings.DataRate}. Track: {settings.RotationSpeedTrack}. Sector: {settings.RotationSpeedSector}. HighResolution: {Stopwatch.IsHighResolution}.");
            Log.Info?.Out($"Измерение скорости вращения диска. Disk Format: {UpperSideHead.Head0}. Side: {DiskSide.Both}. DataRate: {DataRate.FD_RATE_250K}. Track: {10}. Sector: {1}. HighResolution: {Timer.IsHighResolution}.");
            BackgroundWorker worker = new BackgroundWorker();
            int RotationSpeedCount = 100;
            int RotationSpeedTrack = 10;
            int RotationSpeedSector = 1;
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                IntPtr driverHandle = IntPtr.Zero;
                try
                {
                    //aborted = false;
                    //driverHandle = Driver.Open(settings.DataRate);
                    driverHandle = Driver.Open(DataRate.FD_RATE_250K);
                    if ((int)driverHandle == Driver.INVALID_HANDLE_VALUE) return;
                    IntPtr memoryHandle = Driver.VirtualAlloc(256);
                    if (memoryHandle == IntPtr.Zero) return;
                    //MList<TimeSpan> values = new MList<TimeSpan>(settings.RotationSpeedCount);
                    MList<TimeSpan> values = new MList<TimeSpan>(RotationSpeedCount);
                    //int error = Driver.ReadSectorWithSeek(driverHandle, memoryHandle, settings.RotationSpeedTrack, settings.RotationSpeedSector, settings.DiskFormat, true);
                    Driver.Seek(driverHandle, RotationSpeedTrack);
                    int error = Driver.ReadSector(driverHandle, memoryHandle, RotationSpeedTrack, 1, UpperSideHead.Head0, 1);
                    Timer pc = Timer.StartNew();
                    Timer pcTotal = Timer.StartNew();
                    for (int i = 0; i < RotationSpeedCount; i++)
                    {
                        //int error1 = Driver.ReadSectorWithSeek(driverHandle, memoryHandle, settings.RotationSpeedTrack, settings.RotationSpeedSector, settings.DiskFormat, true);
                        int error1 = Driver.ReadSector(driverHandle, memoryHandle, RotationSpeedTrack, RotationSpeedSector, UpperSideHead.Head0, 1);
                        pc.Stop();
                        if ((error1 == 0 || error1 == 23) && (error == 0 || error == 23))
                        {
                            values.Add(pc.Elapsed);
                            double sec = (double)pc.Elapsed.Ticks / TimeSpan.TicksPerSecond;
                            double rpm = sec > 0 ? 1 / sec * 60 : 0;
                            Log.Info?.Out($"Error: {error1} | Time: {GP.ToString(sec * 1000, 2)} ms ({GP.ToString(rpm, 2)} rpm)");
                        }
                        else
                        {
                            Log.Info?.Out($"Error: {error1}");
                        }
                        error = error1;
                        pc.Restart();
                    }
                    pc.Stop();
                    pcTotal.Stop();
                    double method2Rpm = values.Cnt == RotationSpeedCount ? (1 / ((double)pcTotal.Elapsed.Ticks / TimeSpan.TicksPerSecond / values.Cnt) * 60) : 0;
                    TimeSpan min = TimeSpan.MaxValue;
                    TimeSpan max = TimeSpan.MinValue;
                    TimeSpan sum = TimeSpan.Zero;
                    if (values.Cnt == 0)
                    {
                        Log.Info?.Out("Не удалось измерить скорость из-за ошибок чтения.");
                        return;
                    }
                    for (int i = 0; i < values.Cnt; i++)
                    {
                        if (values.Data[i] < min) min = values.Data[i];
                        if (values.Data[i] > max) max = values.Data[i];
                        sum += values.Data[i];
                    }
                    double averageRpm = (1 / ((double)sum.Ticks / TimeSpan.TicksPerSecond / values.Cnt) * 60);
                    double maxRpm = (1 / ((double)min.Ticks / TimeSpan.TicksPerSecond) * 60);
                    double minRpm = (1 / ((double)max.Ticks / TimeSpan.TicksPerSecond) * 60);
                    Log.Info?.Out($"Средняя скорость вращения (об/мин): {GP.ToString(averageRpm, 3)}. Мин: {GP.ToString(minRpm, 3)}. Макс: {GP.ToString(maxRpm, 3)}. | Метод 2: {(method2Rpm > 0 ? GP.ToString(method2Rpm, 3) : "Ошибка чтения")}.");
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение при чтении сектора: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
                finally
                {
                    if (driverHandle != IntPtr.Zero) Driver.Close(driverHandle);
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                MainForm_OperationCompleted(sender, e);
            };
            worker.RunWorkerAsync();

            // оригинал:

            //if (!ReadParameters()) return;
            //EnableControls(true);
            //Log.I?.InfoI($"Измерение скорости вращения диска. Disk Format: {settings.DiskFormat}. Side: {settings.ReadSide}. DataRate: {settings.DataRate}. Track: {settings.RotationSpeedTrack}. Sector: {settings.RotationSpeedSector}. HighResolution: {Stopwatch.IsHighResolution}.");
            //BackgroundWorker worker = new BackgroundWorker();
            //worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            //{
            //    IntPtr driverHandle = IntPtr.Zero;
            //    try
            //    {
            //        aborted = false;
            //        driverHandle = Driver.Open(settings.DataRate);
            //        if ((int)driverHandle == Driver.INVALID_HANDLE_VALUE) return;
            //        IntPtr memoryHandle = Driver.VirtualAlloc(256);
            //        if (memoryHandle == IntPtr.Zero) return;
            //        MList<TimeSpan> values = new MList<TimeSpan>(settings.RotationSpeedCount);
            //        int error = Driver.ReadSectorWithSeek(driverHandle, memoryHandle, settings.RotationSpeedTrack, settings.RotationSpeedSector, settings.DiskFormat, true);
            //        Stopwatch pc = Stopwatch.StartNew();
            //        Stopwatch pcTotal = Stopwatch.StartNew();
            //        for (int i = 0; i < settings.RotationSpeedCount && !aborted; i++)
            //        {
            //            int error1 = Driver.ReadSectorWithSeek(driverHandle, memoryHandle, settings.RotationSpeedTrack, settings.RotationSpeedSector, settings.DiskFormat, true);
            //            pc.Stop();
            //            if ((error1 == 0 || error1 == 23) && (error == 0 || error == 23))
            //            {
            //                values.Add(pc.Elapsed);
            //                double sec = (double)pc.Elapsed.Ticks / TimeSpan.TicksPerSecond;
            //                double rpm = sec > 0 ? 1 / sec * 60 : 0;
            //                Log.I?.InfoI($"Error: {error1} | Time: {GP.ToString(sec * 1000, 2)} ms ({GP.ToString(rpm, 2)} rpm)");
            //            }
            //            else
            //            {
            //                Log.I?.InfoI($"Error: {error1}");
            //            }
            //            error = error1;
            //            pc.Reset();
            //            pc.Start();
            //        }
            //        pc.Stop();
            //        pcTotal.Stop();
            //        double method2Rpm = values.Cnt == settings.RotationSpeedCount ? (1 / ((double)pcTotal.Elapsed.Ticks / TimeSpan.TicksPerSecond / values.Cnt) * 60) : 0;
            //        TimeSpan min = TimeSpan.MaxValue;
            //        TimeSpan max = TimeSpan.MinValue;
            //        TimeSpan sum = TimeSpan.Zero;
            //        if (values.Cnt == 0)
            //        {
            //            Log.I?.InfoI("Не удалось измерить скорость из-за ошибок чтения.");
            //            return;
            //        }
            //        for (int i = 0; i < values.Cnt; i++)
            //        {
            //            if (values.Data[i] < min) min = values.Data[i];
            //            if (values.Data[i] > max) max = values.Data[i];
            //            sum += values.Data[i];
            //        }
            //        double averageRpm = (1 / ((double)sum.Ticks / TimeSpan.TicksPerSecond / values.Cnt) * 60);
            //        double maxRpm = (1 / ((double)min.Ticks / TimeSpan.TicksPerSecond) * 60);
            //        double minRpm = (1 / ((double)max.Ticks / TimeSpan.TicksPerSecond) * 60);
            //        Log.I?.InfoI($"Средняя скорость вращения (об/мин): {GP.ToString(averageRpm, 3)}. Мин: {GP.ToString(minRpm, 3)}. Макс: {GP.ToString(maxRpm, 3)}. | Метод 2: {(method2Rpm > 0 ? GP.ToString(method2Rpm, 3) : "Ошибка чтения")}.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.E?.ErrorI($"Исключение при чтении сектора: {ex.Message} | StackTrace: {ex.StackTrace}");
            //    }
            //    finally
            //    {
            //        if (driverHandle != IntPtr.Zero) Driver.Close(driverHandle);
            //    }
            //};
            //worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            //{
            //    EnableControls(false);
            //};
            //worker.RunWorkerAsync();
        }

        private void C_Help_Click(object sender, EventArgs e)
        {
            if (helpForm == null) helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        private void C_ScanDisk_Click(object sender, EventArgs e)
        {
            TrDosReader reader = (TrDosReader)readers[0];
            if (!reader.ReadParameters(false)) return;
            Log.Info?.Out($"Сканирование формата диска. DataRate: {reader.Params.DataRate}");
            C_Abort.Enabled = true;
            diskReader = new DiskReader((DiskReaderParams)reader.Params.Clone());
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    diskReader.OpenDriver();
                    diskReader.ScanDiskFormat(diskFormat);
                    diskReader.CloseDriver();
                    Log.Info?.Out($"Найдено секторов: {TrackFormat.TotalSectors(diskFormat)}");
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение при сканировании диска: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                C_Abort.Enabled = false;
            };
            worker.RunWorkerAsync();
        }

        private void C_SaveDiskFormat_Click(object sender, EventArgs e)
        {
            if (diskFormat == null) return;
            SaveFileDialog saveDialog = new SaveFileDialog() { Filter = "Xml Files (*.xml)|*.xml|All Files (*.*)|*.*" };
            saveDialog.FileName = "Disk Format";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(saveDialog.FileName, TrackFormat.ToXml(diskFormat));
        }

        private void C_NewDiskFormat_Click(object sender, EventArgs e)
        {
            diskFormat = new MList<TrackFormat>(172);
            for (int i = 0; i < 172; i++)
            {
                diskFormat.Add(new TrackFormat(55));
            }
        }

        private void C_LoadDiskFormat_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "Xml Files (*.xml)|*.xml|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            diskFormat = TrackFormat.LoadXml(openDialog.FileName);
        }

        private void C_UIRefreshPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Interval = Int32.Parse(C_UIRefreshPeriod.Text);
        }

        private void C_Abort_Click(object sender, EventArgs e)
        {
            if (diskReader != null) diskReader.Aborted = true;
        }

        private string GetPattern(string fileName, out int numLen)
        {
            int index = fileName.IndexOf('*');
            numLen = 0;
            if (index < 0) return fileName + "[0-9]+$";
            int i;
            for (i = index + 1; i < fileName.Length; i++)
            {
                if (fileName[i] != '*') break;
            }
            numLen = i - index;
            return "^" + fileName.Remove(index, numLen).Insert(index, "[0-9]+").Replace("*", "") + "$";
        }

        private void C_NewDisk_Click(object sender, EventArgs e)
        {
            if (!HtReadParameters())
            {
                MessageBox.Show("Ошибка в параметрах.");
                return;
            }
            htTabPars.Processing = true;
            MainForm_OperationStarted(htTabPars, e);
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    htTabPars.Image = null;
                    htTabPars.Map.Image = null;
                    htTabPars.StatsTable.Image = null;
                    htTabPars.Params = new DiskReaderParams()
                    {
                        DataRate = htTabPars.DataRate,
                        SectorNumFrom = 0,
                        SectorReadAttempts = htTabPars.SectorReadAttempts,
                        Side = DiskSide.Both,
                        TrackFrom = 0,
                        TrackTo = htTabPars.MaxTracks,
                        UpperSideHead = UpperSideHead.Head1
                    };

                    diskReader = new DiskReader(htTabPars.Params);
                    diskReader.OpenDriver();
                    bool formatIdentified = false;
                    for (int i = 0; i < 3; i++)
                    {
                        Driver.Recalibrate(diskReader.DriverHandle);
                        bool scanResult = diskReader.ScanFormat(htTabPars.Track0Format, 0, true);
                        if (diskReader.Aborted) goto abort;
                        Log.Info?.Out($"Формат 0 трека: {htTabPars.Track0Format.FormatName} | {htTabPars.Track0Format.Layout.Cnt} sectors | {htTabPars.Track0Format.ToStringAsSectorArray()}");
                        if (htTabPars.Track0Format.DoesSatisfyFormat(TrackFormat.TrDos, 0))
                        {
                            if (htTabPars.TrDosChecked)
                            {
                                htTabPars.Map.ChangeSize(256, 16);
                                htTabPars.Image = new TrDosImage(htTabPars.DefaultImageSizeInTracks * 16, htTabPars.Map);
                                Log.Info?.Out($"Reading as TR-DOS.");
                            }
                            else
                            {
                                Log.Info?.Out($"Detected as TR-DOS.");
                                goto abort;
                            }
                        }
                        else if (htTabPars.Track0Format.FormatName >= TrackFormatName.CpmSequential && htTabPars.Track0Format.FormatName <= TrackFormatName.CpmGeneric)
                        {
                            if (htTabPars.CpmChecked)
                            {
                                htTabPars.Map.ChangeSize(1024, 5);
                                htTabPars.Image = new CpmImage(htTabPars.DefaultImageSizeInTracks * 5, htTabPars.Map);
                                Log.Info?.Out($"Reading as CP/M.");
                            }
                            else
                            {
                                Log.Info?.Out($"Detected as CP/M.");
                                goto abort;
                            }
                        }
                        else if (htTabPars.Track0Format.FormatName >= TrackFormatName.IsDosSequential && htTabPars.Track0Format.FormatName <= TrackFormatName.IsDosGeneric)
                        {
                            if (htTabPars.IsDosChecked)
                            {
                                htTabPars.Map.ChangeSize(1024, 5);
                                htTabPars.Image = new IsDosImage(htTabPars.DefaultImageSizeInTracks * 5, htTabPars.Map);
                                Log.Info?.Out($"Reading as IS-DOS.");
                            }
                            else
                            {
                                Log.Info?.Out($"Detected as IS-DOS.");
                                goto abort;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        formatIdentified = true;
                        break;
                    }
                    if (htTabPars.Track0Format.Layout.Cnt == 0)
                    {
                        Log.Info?.Out($"На нулевом треке заголовки секторов не обнаружены.");
                        goto abort;
                    }
                    else if (!formatIdentified)
                    {
                        Log.Info?.Out($"Формат не поддерживается или диск имеет ошибки на нулевом треке.");
                        goto abort;
                    }
                    htTabPars.Map.Image = htTabPars.Image;
                    htTabPars.StatsTable.Image = htTabPars.Image;
                    htTabPars.Params.ReadMode = htTabPars.Image is TrDosImage && Timer.IsHighResolution ? ReadMode.Fast : ReadMode.Standard;
                    htTabPars.Params.TrackLayoutAutodetect = htTabPars.Image is TrDosImage;
                    htTabPars.Params.UpperSideHeadAutodetect = htTabPars.Image is TrDosImage;
                    htTabPars.CurrentFileName = null;
                    htTabPars.StatsTable.Image = htTabPars.Image;
                    htTabPars.Params.Image = htTabPars.Image;
                    HtRead(false);
                    if (!diskReader.Aborted) HtSave();
                    return;
                    abort:
                    diskReader.CloseDriver();
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                htTabPars.Processing = false;
                MainForm_OperationCompleted(sender, e);
            };
            worker.RunWorkerAsync();
        }

        private void C_RepeatReading_Click(object sender, EventArgs e)
        {
            if(!HtReadParameters())
            {
                MessageBox.Show("Ошибка в параметрах.");
                return;
            }
            htTabPars.Processing = true;
            MainForm_OperationStarted(htTabPars, e);
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (object sndr, DoWorkEventArgs ee) =>
            {
                try
                {
                    htTabPars.Params.DataRate = htTabPars.DataRate;
                    htTabPars.Params.SectorReadAttempts = htTabPars.SectorReadAttempts;
                    htTabPars.Params.TrackTo = htTabPars.MaxTracks;
                    HtRead(true);
                    if(!diskReader.Aborted) HtSave();
                }
                catch (Exception ex)
                {
                    Log.Error?.Out($"Исключение: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            };
            worker.RunWorkerCompleted += (object sender1, RunWorkerCompletedEventArgs ee) =>
            {
                htTabPars.Processing = false;
                MainForm_OperationCompleted(sender, e);
            };
            worker.RunWorkerAsync();
        }

        private void C_HtSave_Click(object sender, EventArgs e)
        {
            HtSave();
        }

        private void HtRead(bool newDiskReader)
        {
            if (newDiskReader)
            {
                htTabPars.Params = new DiskReaderParams()
                {
                    Image = htTabPars.Image,
                    DataRate = htTabPars.DataRate,
                    SectorNumFrom = 0,
                    SectorReadAttempts = htTabPars.SectorReadAttempts,
                    Side = DiskSide.Both,
                    TrackFrom = 0,
                    TrackTo = htTabPars.MaxTracks,
                    UpperSideHead = UpperSideHead.Head1,
                    TrackLayoutAutodetect = htTabPars.Image is TrDosImage
                };

                htTabPars.Params.UpperSideHeadAutodetect = htTabPars.Image is TrDosImage;
                diskReader = new DiskReader(htTabPars.Params);
                diskReader.OpenDriver();
            }
            diskReader.Params.SectorNumTo = Math.Min(htTabPars.Image.SizeSectors, htTabPars.MaxTracks * htTabPars.Image.StandardFormat.Layout.Cnt);
            diskReader.Params.CurrentTrackFormat = htTabPars.Track0Format;
            for (int readCntr = 0; readCntr < htTabPars.NumberOfReads; readCntr++)
            {
                diskReader.ReadForward();
                if (diskReader.Aborted) goto end;
                if (readCntr == 0)
                {
                    diskReader.DetectDiskSize(htTabPars.MaxTracks);
                    if (diskReader.Aborted) goto end;
                }
                readCntr++;
                if (readCntr >= htTabPars.NumberOfReads || htTabPars.Image.BadSectors == 0) break;
                diskReader.ReadBackward();
                if (diskReader.Aborted) goto end;
                if (htTabPars.Image.BadSectors == 0) break;
            }
            if (htTabPars.RandomReadTurnedOn && htTabPars.Image.BadSectors > 0)
            {
                diskReader.ReadRandomSectors(htTabPars.RandomReadTimeout, htTabPars.RandomReadStopOnNthFail);
                if (diskReader.Aborted) goto end;
            }
            end:
            diskReader.CloseDriver();            
        }

        private void HtSave()
        {
            string savePath = null;
            FileType fileType = FileType.Fdi;
            string fileName = null;
            if (htTabPars.Image is TrDosImage)
            {
                savePath = htTabPars.TrDosDir;
                fileType = htTabPars.TrDosFileType;
                fileName = htTabPars.TrDosFileName;
            }
            else if (htTabPars.Image is IsDosImage)
            {
                savePath = htTabPars.IsDosDir;
                fileType = htTabPars.IsDosFileType;
                fileName = htTabPars.IsDosFileName;
            }
            else if (htTabPars.Image is CpmImage)
            {
                savePath = htTabPars.CpmDir;
                fileType = htTabPars.CpmFileType;
                fileName = htTabPars.CpmFileName;
            }
            else
            {
                throw new Exception();
            }
            if (!Directory.Exists(savePath))
            {
                Log.Error?.Out($"Директория для сохранения файлов не существует: {savePath}. Файл не сохранен.");
                return;
            }
            string nFileName;
            if (htTabPars.CurrentFileName == null)
            {
                string[] files = Directory.GetFiles(savePath);
                int numLen;
                string pattern = GetPattern(fileName, out numLen);
                Regex regex = new Regex(pattern);
                int maxNumber = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    string curFileName = Path.GetFileNameWithoutExtension(files[i]);
                    if (!regex.IsMatch(curFileName)) continue;
                    string[] parts = Regex.Split(curFileName, @"\D+");
                    for (int j = 0; j < parts.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(parts[j]))
                        {
                            int number = int.Parse(parts[j]);
                            if (number > maxNumber) maxNumber = number;
                            break;
                        }
                    }
                }
                string numAsString = (maxNumber + 1).ToString("D" + numLen);
                nFileName = fileName.Replace(new string('*', numLen), numAsString);
            }
            else
            {
                nFileName = htTabPars.CurrentFileName;
            }

            string fullFileName;
            switch (fileType)
            {
                case FileType.Fdi:
                    fullFileName = Path.Combine(savePath, Path.ChangeExtension(nFileName, ".fdi"));
                    File.WriteAllBytes(fullFileName, htTabPars.Image.ToFdi(null, diskReader.Params.SectorsOnTrack * 2));
                    break;

                case FileType.Trd:
                case FileType.ModifiedTrd:
                    fullFileName = Path.Combine(savePath, Path.ChangeExtension(nFileName, ".trd"));
                    File.WriteAllBytes(fullFileName, ((TrDosImage)htTabPars.Image).ToTrd(diskReader.Params.SectorsOnTrack * 2, fileType == FileType.ModifiedTrd));
                    break;

                case FileType.Kdi:
                    fullFileName = Path.Combine(savePath, Path.ChangeExtension(nFileName, ".kdi"));
                    File.WriteAllBytes(fullFileName, ((CpmImage)htTabPars.Image).ToKdi(diskReader.Params.SectorsOnTrack * 2));
                    break;

                default:
                    throw new Exception();
            }
            htTabPars.CurrentFileName = fullFileName;
            htTabPars.Image.ResetModify();
            htTabPars.Image.Name = nFileName;
            Log.Info?.Out($"Файл сохранен. Sectors: {htTabPars.Image.FileSectorsSize} ({Math.Ceiling((double)htTabPars.Image.FileSectorsSize / htTabPars.Image.SectorsOnTrack)} tracks) | Good: {htTabPars.Image.GoodSectors} | Bad: {htTabPars.Image.BadSectors} | Имя: {fullFileName}");
        }

        private void HtRefreshControls()
        {
            if (htTabPars.Image != null)
            {
                htTabPars.Map.Repaint();
                htTabPars.StatsTable.Repaint();
            }
        }

        private void C_HtAbort_Click(object sender, EventArgs e)
        {
            if (diskReader != null) diskReader.Aborted = true;
            htTabPars.Processing = false;
            HtSetEnabled(true);
        }

        private void C_SelectSavePathTrDos_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = htTabPars.TrDosDir;
            if (folderDialog.ShowDialog() != DialogResult.OK) return;
            htTabPars.TrDosDir = folderDialog.SelectedPath;
        }

        private void C_SelectSavePathIsDos_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = htTabPars.IsDosDir;
            if (folderDialog.ShowDialog() != DialogResult.OK) return;
            htTabPars.IsDosDir = folderDialog.SelectedPath;
        }

        private void C_SelectSavePathCpm_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = htTabPars.CpmDir;
            if (folderDialog.ShowDialog() != DialogResult.OK) return;
            htTabPars.CpmDir = folderDialog.SelectedPath;
        }

        protected void HtSetEnabled(bool parsValid)
        {
            C_NewDisk.Enabled = htTabPars.Enabled && !htTabPars.Processing && parsValid;
            C_RepeatReading.Enabled = htTabPars.Enabled && !htTabPars.Processing && htTabPars.Image != null && parsValid;
            C_HtSave.Enabled = htTabPars.Enabled && !htTabPars.Processing && htTabPars.Image != null && parsValid;
            C_HtAbort.Enabled = htTabPars.Enabled && htTabPars.Processing;
        }

        public bool HtReadParameters()
        {
            int trackTo;
            bool sraValid = Int32.TryParse(C_HtSectorReadAttempts.Text, out htTabPars.SectorReadAttempts) && htTabPars.SectorReadAttempts > 0;
            bool tlValid = Int32.TryParse(C_HtMaxTracks.Text, out trackTo) && trackTo > 0 && trackTo <= MainForm.MaxTrack;
            C_HtSectorReadAttempts.BackColor = sraValid ? SystemColors.Window : Color.Red;
            C_HtMaxTracks.BackColor = tlValid ? SystemColors.Window : Color.Red;
            if (tlValid) htTabPars.Map.TrackTo = trackTo;
            htTabPars.Map.Repaint();
            htTabPars.MaxTracks = trackTo;
            htTabPars.DataRate = ReaderBase.DataRateArray[C_HtDataRate.SelectedIndex];
            bool disValid = Int32.TryParse(C_HtDefaultImageSize.Text, out htTabPars.DefaultImageSizeInTracks)
                && htTabPars.DefaultImageSizeInTracks > 0
                && htTabPars.DefaultImageSizeInTracks <= 172;
            C_HtDefaultImageSize.BackColor = disValid ? SystemColors.Window : Color.Red;
            bool numberOfReadsValid = Int32.TryParse(C_HtNumberOfReads.Text, out htTabPars.NumberOfReads) && htTabPars.NumberOfReads >= 0;
            C_HtNumberOfReads.BackColor = numberOfReadsValid ? SystemColors.Window : Color.Red;
            htTabPars.TrDosChecked = C_HtTrDosCheckBox.Checked;
            htTabPars.IsDosChecked = C_HtIsDosCheckBox.Checked;
            htTabPars.CpmChecked = C_HtCpmCheckBox.Checked;
            htTabPars.TrDosFileName = C_FilePatternTrDos.Text;
            htTabPars.IsDosFileName = C_FilePatternIsDos.Text;
            htTabPars.CpmFileName = C_FilePatternCpm.Text;
            htTabPars.TrDosFileType = HtTabPars.TrDosFileTypes[C_FileTypeTrDos.SelectedIndex];
            htTabPars.IsDosFileType = HtTabPars.IsDosFileTypes[C_FileTypeIsDos.SelectedIndex];
            htTabPars.CpmFileType = HtTabPars.CpmFileTypes[C_FileTypeCpm.SelectedIndex];
            htTabPars.RandomReadTurnedOn = C_HtRandomReadOn.Checked;
            bool timeoutValid = TimeSpan.TryParse(C_HtTimeout.Text, out htTabPars.RandomReadTimeout);
            C_HtTimeout.BackColor = timeoutValid ? SystemColors.Window : Color.Red;
            bool stopOnnthFailValid = Int32.TryParse(C_HtStopOnNthFail.Text, out htTabPars.RandomReadStopOnNthFail) & htTabPars.RandomReadStopOnNthFail >= 0;
            C_HtStopOnNthFail.BackColor = stopOnnthFailValid ? SystemColors.Window : Color.Red;
            return sraValid && tlValid && disValid && numberOfReadsValid && stopOnnthFailValid && timeoutValid;
        }

        public void HtFillParameters()
        {
            htTabPars.ParametersChanging = true;
            C_HtTrDosCheckBox.Checked = htTabPars.TrDosChecked;
            C_HtIsDosCheckBox.Checked = htTabPars.IsDosChecked;
            C_HtCpmCheckBox.Checked = htTabPars.CpmChecked;
            C_FilePatternTrDos.Text = htTabPars.TrDosFileName;
            C_FilePatternIsDos.Text = htTabPars.IsDosFileName;
            C_FilePatternCpm.Text = htTabPars.CpmFileName;
            C_FileTypeTrDos.SelectedIndex = HtTabPars.GetFileTypeIndex(htTabPars.TrDosFileType, HtTabPars.TrDosFileTypes);
            C_FileTypeIsDos.SelectedIndex = HtTabPars.GetFileTypeIndex(htTabPars.IsDosFileType, HtTabPars.IsDosFileTypes);
            C_FileTypeCpm.SelectedIndex = HtTabPars.GetFileTypeIndex(htTabPars.CpmFileType, HtTabPars.CpmFileTypes);
            C_HtDataRate.SelectedIndex = HtTabPars.GetDataRateIndex(htTabPars.DataRate, ReaderBase.DataRateArray);
            C_HtSectorReadAttempts.Text = htTabPars.SectorReadAttempts.ToString();
            C_HtDefaultImageSize.Text = htTabPars.DefaultImageSizeInTracks.ToString();
            C_HtMaxTracks.Text = htTabPars.MaxTracks.ToString();
            C_HtNumberOfReads.Text = htTabPars.NumberOfReads.ToString();
            C_HtRandomReadOn.Checked = htTabPars.RandomReadTurnedOn;
            C_HtTimeout.Text = htTabPars.RandomReadTimeout.Hours.ToString("D2") + ":" + htTabPars.RandomReadTimeout.Minutes.ToString("D2") + ":"
                + htTabPars.RandomReadTimeout.Seconds.ToString("D2");
            C_HtStopOnNthFail.Text = htTabPars.RandomReadStopOnNthFail.ToString();
            htTabPars.ParametersChanging = false;
        }

        private void C_FilePatternTrDos_TextChanged(object sender, EventArgs e)
        {
            if (htTabPars.ParametersChanging) return;
            HtSetEnabled(HtReadParameters());
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (tabControl1.SelectedIndex == 5 && C_NewDisk.Enabled && keyData == Keys.F4)
            {
                C_NewDisk_Click(null, null);
            }
            if (tabControl1.SelectedIndex == 5 && C_HtAbort.Enabled && keyData == Keys.Escape)
            {
                C_HtAbort_Click(null, null);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
