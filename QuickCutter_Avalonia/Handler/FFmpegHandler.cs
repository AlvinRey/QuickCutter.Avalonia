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

        static public async Task<IMediaAnalysis> AnaliysisMedia(string mediaFullName)
        {
            return await FFProbe.AnalyseAsync(mediaFullName);
        }

        static public string GetStreamName(MediaStream stream, int index)
        {
            string title = string.Empty;
            string language = string.Empty;

            if (stream.Tags.ContainsKey("title"))
            {
                title = stream.Tags["title"];
            }
            else if (stream.Tags.ContainsKey("Title"))
            {
                title = stream.Tags["Title"];
            }
            else if (stream.Tags.ContainsKey("TITLE"))
            {
                title = stream.Tags["TITLE"];
            }
            else
            {
                title = $"Track {index + 1}";
            }


            if (!string.IsNullOrEmpty(stream.Language))
            {
                language = Utils.ISO639_2_Converter.ContainsKey(stream.Language) ? Utils.ISO639_2_Converter[stream.Language] : stream.Language;

                title += $" - [{language}]";
            }
            return title;
        }
    }
}