using Avalonia.Threading;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;

namespace QuickCutter_Avalonia.Handler
{
    internal class LogHandler
    {
        #region  Private Member

        private static string? mLogDirectory;
        private static string? mFileName;
        private static string? mFileFullName;
        private static string? mLogMsg;
        #endregion

        public static void SetUp()
        {
            MessageBus.Current.Listen<string>("LogHandler").Subscribe(x => { mLogMsg = x; DelegateAppendText(); });
            mLogDirectory = Utility.GetLogPath();
            mFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            mFileFullName = Path.Combine(mLogDirectory, mFileName);
            mLogMsg = "================== Quick Cutter Start =========================";
            OutputLog();
        }

        private static void DelegateAppendText()
        {
            Dispatcher.UIThread.Invoke(OutputLog, DispatcherPriority.Send);
        }

        private static void OutputLog()
        {
            if (string.IsNullOrEmpty(mFileFullName) || string.IsNullOrEmpty(mLogMsg))
            {
                return;
            }

            using (StreamWriter sw = File.AppendText(mFileFullName))
            {
                if (!mLogMsg.EndsWith(Environment.NewLine))
                {
                    mLogMsg += Environment.NewLine;
                }
                sw.WriteLine(mLogMsg);
            }
        }
    }
}
