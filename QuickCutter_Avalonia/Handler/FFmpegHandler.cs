using FFMpegCore;
using QuickCutter_Avalonia.Mode;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.IO;
using ReactiveUI;


namespace QuickCutter_Avalonia.Handler
{
    internal class FFmpegHandler
    {
        static public void CheckFFmpegIsExist()
        {
            bool isFFmpegExist = false;
            if (File.Exists(Utils.GetFFmpegPath("ffmpeg.exe")) && File.Exists(Utils.GetFFmpegPath("ffprobe.exe")))
            {
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Utils.GetFFmpegPath(), TemporaryFilesFolder = Utils.GetFFmpegTempPath() });
                isFFmpegExist = true;
            }
            else
            {
                Utils.SaveLog($"Can not find ffmpeg.exe and ffprobe.exe in {Utils.GetFFmpegPath()}");
            }

            if(!isFFmpegExist)
            {
                // Try to find FFmpeg in ednvironment path
                string? pathVariable = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathVariable))
                {
                    //var paths = pathVariable.Split(';').Where(path => path.Contains("ffmpeg"));
                    //if (paths != null)
                    //{
                    //    foreach (var path in paths)
                    //    {
                    //        containsFFmpeg = File.Exists(Path.Combine(path, "ffmpeg.exe")) && File.Exists(Path.Combine(path, "ffprobe.exe"));
                    //        if (containsFFmpeg)
                    //        {
                    //            break;
                    //        }
                    //    }
                    //}
                    isFFmpegExist = pathVariable.Split(';').Select(path => File.Exists(Path.Combine(path, "ffmpeg.exe")) && File.Exists(Path.Combine(path, "ffprobe.exe"))).Any(boolean => boolean == true);
                }
            }

            if (!isFFmpegExist)
            {
                Utils.SaveLog("Can not find ffmpeg.exe and ffprobe.exe in Environment PATH");
                throw new Exception($@"Can not find ffmpeg.exe or ffprobe.exe in Environment PATH or in ""{Utils.GetFFmpegPath()}""");
            }
            else
            {
                Utils.SaveLog("Success to find FFmepg");
            }
        }

        static public async Task<IMediaAnalysis> AnaliysisMedia(string mediaFullName)
        {
            return await FFProbe.AnalyseAsync(mediaFullName);
        }
    }
}