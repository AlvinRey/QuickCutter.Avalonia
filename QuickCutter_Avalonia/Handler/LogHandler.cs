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
        private static StreamWriter? mSw;
        #endregion

        public static void Init()
        {
            MessageBus.Current.Listen<string>(Global.LogTarget).Subscribe(message => OutputLog(message));
            mLogDirectory = Utils.GetLogPath();
            var fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            var fileFullName = Path.Combine(mLogDirectory, fileName);
            mSw = File.AppendText(fileFullName);
        }

        public static void Dispose()
        {
            mSw?.Dispose();
        }

        private static void OutputLog(string message)
        {
            if (mSw is null || string.IsNullOrEmpty(message)) return;

            message = DateTime.Now.ToString() + ' ' + message;
            if (!message.EndsWith(Environment.NewLine))
            {
                message += Environment.NewLine;
            }
            mSw.WriteLineAsync(message);
        }
    }
}
