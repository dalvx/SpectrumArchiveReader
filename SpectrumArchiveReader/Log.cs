using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class Log
    {
        public static Log Trace;
        public static Log Info;
        public static Log Debug;
        public static Log Warn;
        public static Log Error;
        public static Log Fatal;
        private static bool initialized;
        public static StreamWriter LogFile;
        public static bool FileOutput;
        public static bool WinOutput;
        private static MList<string> winOutputNormal;
        private static MList<string> winOutputError;
        private static object thisLock = new object();
        public static string LogPath;
        private static MsgType allowedMessagesFile;
        private static MsgType allowedMessagesWindow;
        public static MsgType AllowedMessagesFile
        {
            get { return allowedMessagesFile; }

            set
            {
                allowedMessagesFile = value;
                SetShortcuts();
            }
        }
        public static MsgType AllowedMessagesWindow
        {
            get { return allowedMessagesWindow; }

            set
            {
                allowedMessagesWindow = value;
                SetShortcuts();
            }
        }
        private MsgType msgType;

        [Flags]
        public enum MsgType
        {
            Trace = 0x01,
            Info = 0x02,
            Debug = 0x04,
            Warn = 0x08,
            Error = 0x10,
            Fatal = 0x20,
            All = Trace | Info | Debug | Warn | Error | Fatal
        }

        public Log(MsgType msgType)
        {
            this.msgType = msgType;
        }

        private static void SetShortcuts()
        {
            Trace = ((allowedMessagesFile & MsgType.Trace) != 0 && FileOutput) || ((allowedMessagesWindow & MsgType.Trace) != 0 && WinOutput) ? new Log(MsgType.Trace) : null;
            Info = ((allowedMessagesFile & MsgType.Info) != 0 && FileOutput) || ((allowedMessagesWindow & MsgType.Info) != 0 && WinOutput) ? new Log(MsgType.Info) : null;
            Debug = ((allowedMessagesFile & MsgType.Debug) != 0 && FileOutput) || ((allowedMessagesWindow & MsgType.Debug) != 0 && WinOutput) ? new Log(MsgType.Debug) : null;
            Warn = ((allowedMessagesFile & MsgType.Warn) != 0 && FileOutput) || ((allowedMessagesWindow & MsgType.Warn) != 0 && WinOutput) ? new Log(MsgType.Warn) : null;
            Error = ((allowedMessagesFile & MsgType.Error) != 0 && FileOutput) || ((allowedMessagesWindow & MsgType.Error) != 0 && WinOutput) ? new Log(MsgType.Error) : null;
            Fatal = ((allowedMessagesFile & MsgType.Fatal) != 0 && FileOutput) || ((allowedMessagesWindow & MsgType.Fatal) != 0 && WinOutput) ? new Log(MsgType.Fatal) : null;
        }

        public static bool Init(bool initFile, bool fileOutput, bool windowOutput, MList<string> windowOutputNormalx, MList<string> windowOutputErrorx, MsgType allowedMessagesFile, MsgType allowedMessagesWindow)
        {
            if (initialized) return true;
            Log.allowedMessagesFile = allowedMessagesFile;
            Log.allowedMessagesWindow = allowedMessagesWindow;
            FileOutput = fileOutput;
            WinOutput = windowOutput;
            winOutputNormal = windowOutputNormalx;
            winOutputError = windowOutputErrorx;
            SetShortcuts();
            LogFile = null;
            if (initFile)
            {
                LogPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Log\";
                string s = LogPath + "SpectrumArchiveReader " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff") + ".log";
                if (!Directory.Exists(LogPath))
                {
                    try
                    {
                        DirectoryInfo di = Directory.CreateDirectory(LogPath);
                    }
                    catch (Exception e)
                    {
                        WriteMessage("При попытке создания директории Log возникло исключение: " + e.Message, MsgType.Fatal, true, false);
                        return false;
                    }
                }
                try
                {
                    LogFile = new StreamWriter(s);
                    Trace?.Out($"Log initialized. Date: {DateTime.Now.ToString("dd.MM.yyyy")}. Version: {typeof(Log).Assembly.GetName().Version}");
                }
                catch (Exception e)
                {
                    WriteMessage("При попытке создания лог-файла возникло исключение: " + e.Message, MsgType.Fatal, true, false);
                    return false;
                }
            }
            initialized = true;
            return true;
        }

        private static void WriteMessage(string msg, MsgType msgType, bool window = true, bool file = true)
        {
            StringBuilder sb = new StringBuilder(DateTime.Now.ToString("HH:mm:ss.fff"));
            sb.Append(": ");
            switch (msgType)
            {
                case MsgType.Debug:
                    sb.Append("DEBUG: ");
                    break;

                case MsgType.Error:
                    sb.Append("ERROR: ");
                    break;

                case MsgType.Fatal:
                    sb.Append("FATAL ERROR: ");
                    break;

                case MsgType.Warn:
                    sb.Append("WARNING: ");
                    break;
            }
            sb.Append(msg);
            string s = sb.ToString();
            if (file && FileOutput && (allowedMessagesFile & msgType) != 0)
            {
                lock (thisLock)
                {
                    LogFile.WriteLine(s);
                    LogFile.Flush();
                }
            }
            if (window && WinOutput && (allowedMessagesWindow & msgType) != 0)
            {
                if (winOutputNormal != null)
                {
                    lock (winOutputNormal) winOutputNormal.Add(s);
                }
                if (winOutputError != null && msgType != MsgType.Info && msgType != MsgType.Trace)
                {
                    lock (winOutputError) winOutputError.Add(s);
                }
            }
        }

        public void Out(string msg)
        {
            if ((FileOutput && (allowedMessagesFile & msgType) != 0)
                || (WinOutput && (allowedMessagesWindow & msgType) != 0))
            {
                StackFrame st = new StackFrame(1);
                MethodBase callingMethod = st.GetMethod();
                WriteMessage(callingMethod.DeclaringType.Name + '.' + callingMethod.Name + ": " + msg, msgType);
            }
        }
    }
}
