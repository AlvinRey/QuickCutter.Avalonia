using FFMpegCore;
using QuickCutter_Avalonia.Models;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.IO;
using ReactiveUI;


namespace QuickCutter_Avalonia.Handler
{
    internal static class FFmpegHandler
    {
        public static void CheckFFmpegIsExist()
        {
            // Try to find FFmpeg in Utils.GetFFmpegPath()
            if (File.Exists(Utils.GetFFmpegPath("ffmpeg.exe")) && File.Exists(Utils.GetFFmpegPath("ffprobe.exe")))
            {
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Utils.GetFFmpegPath(), TemporaryFilesFolder = Utils.GetFFmpegTempPath() });
                Utils.SaveLog($"Success to find ffmpeg.exe and ffprobe.exe in \"{Utils.GetFFmpegPath()}\"");
                return;
            }

            Utils.SaveLog($"Can not find ffmpeg.exe and ffprobe.exe in \"{Utils.GetFFmpegPath()}\"");

            // Try to find FFmpeg in Environment PATH
            string? pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathVariable))
            {
                if (pathVariable.Split(';').Select(path => File.Exists(Path.Combine(path, "ffmpeg.exe")) && File.Exists(Path.Combine(path, "ffprobe.exe"))).Any(boolean => boolean == true))
                {
                    Utils.SaveLog($"Success to find ffmpeg.exe and ffprobe.exe in Environment PATH");
                    return;
                }
            }

            throw new Exception($"Can not find ffmpeg.exe or ffprobe.exe in Environment PATH or in \"{Utils.GetFFmpegPath()}\"");
        }

        public static async Task<IMediaAnalysis> AnaliysisMedia(string mediaFullName)
        {
            return await FFProbe.AnalyseAsync(mediaFullName);
        }

        public static string GetStreamName(MediaStream stream, int index)
        {
            string? title = null;
            string[] keyList = new[] { "title", "Title", "TITLE" };

            if (stream.Tags is not null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if(stream.Tags.TryGetValue(keyList[i], out title))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(title))
            {
                title = $"Track " + (index + 1);
            }
            
            if (!string.IsNullOrEmpty(stream.Language))
            {
                Utils.ISO639_2_Converter.TryGetValue(stream.Language, out var language);
                title += string.IsNullOrEmpty(language) ? string.Empty : " - [" + language + ']';
            }
            return title;
        }
    }
}