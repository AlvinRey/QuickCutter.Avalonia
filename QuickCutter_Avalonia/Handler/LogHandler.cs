using Avalonia.Threading;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class LogHandler
    {
        #region  Private Member

        private static string? mLogDirectory;
        private static string? mLogMsg;
        private static StreamWriter? mSw;
        #endregion

        public static void Init()
        {
            MessageBus.Current.Listen<string>("LogHandler").Subscribe(x => { mLogMsg = x; DelegateAppendText(); });
            mLogDirectory = Utility.GetLogPath();
            var fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            var fileFullName = Path.Combine(mLogDirectory, fileName);
            mSw = File.AppendText(fileFullName);
        }

        public static void Dispose()
        {
            mSw?.Dispose();
        }

        private static void DelegateAppendText()
        {
            Dispatcher.UIThread.Invoke(OutputLog, DispatcherPriority.Send);
        }

        private static void OutputLog()
        {
            if (mSw is null || string.IsNullOrEmpty(mLogMsg)) return;

            mLogMsg = DateTime.Now.ToString() + ' ' + mLogMsg;
            if (!mLogMsg.EndsWith(Environment.NewLine))
            {
                mLogMsg += Environment.NewLine;
            }
            mSw.WriteLineAsync(mLogMsg);
        }
    }
}
