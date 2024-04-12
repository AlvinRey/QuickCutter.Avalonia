using FFMpegCore;
using FFMpegCore.Arguments;
using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    public class HideBannerArgument : IArgument
    {
        public string Text => "-hide_banner";
    }

    public class CopyTimeSpanArgument : IArgument
    {
        public string Text => "-copyts";
    }

    public class Mov_text_CodecArgument : IArgument
    {
        public string Text => "-c:s mov_text";
    }

    public class MapAllVideoTrackArgument : IArgument
    {
        public string Text => "-map 0:v";
    }

    public class MapAudioTrackArgument : IArgument
    {
        public readonly int RelativeIndex;
        public string Text => $"-map 0:a:{RelativeIndex}";
        public MapAudioTrackArgument(int relativeIndex)
        {
            RelativeIndex = relativeIndex;
        }
    }

    public class MapSubtitleTrackArgument : IArgument
    {
        public readonly int RelativeIndex;
        public string Text => $"-map 0:s:{RelativeIndex}";
        public MapSubtitleTrackArgument(int relativeIndex)
        {
            RelativeIndex = relativeIndex;
        }
    }

    public class SeekSecondArgument : IArgument
    {
        public readonly double? SeekTo;

        public string Text
        {
            get
            {
                if (!SeekTo.HasValue)
                {
                    return string.Empty;
                }

                return "-ss " + SeekTo.Value.ToString();
            }
        }

        public SeekSecondArgument(double? seekTo)
        {
            SeekTo = seekTo;
        }
    }

    public class EndSeekSecondArgument : IArgument
    {
        public readonly double SeekTo;

        public string Text
        {
            get => "-t " + SeekTo.ToString();
        }

        public EndSeekSecondArgument(double seekTo)
        {
            SeekTo = seekTo;
        }
    }

    public class VF_SubtitlesArgument : IArgument
    {
        public readonly string? MediaFullName;
        public readonly int SubtitleRelativeIndex;

        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(MediaFullName))
                    return string.Empty;
                return $"-vf subtitles=\"{MediaFullName}\":si={SubtitleRelativeIndex}";
            }

        }

        public VF_SubtitlesArgument(string? mediaFullName, int subtitleRelativeIndex)
        {
            MediaFullName = mediaFullName;
            SubtitleRelativeIndex = subtitleRelativeIndex;
        }
    }

    public class CVF_OverlayArgument : IArgument
    {
        public readonly int SubtitleRelativeIndex;

        public string Text
        {
            get => $"-filter_complex \"[0:v][0:s:{SubtitleRelativeIndex}] overlay=x=(W-w)/2:H-h-10\" ";

        }

        public CVF_OverlayArgument(int subtitleRelativeIndex)
        {
            SubtitleRelativeIndex = subtitleRelativeIndex;
        }
    }

    internal class ExportHandler
    {
        static private Action? mCencelAction;

        static public void CencelExport()
        {
            mCencelAction?.Invoke();
        }

        static private FFMpegArgumentProcessor GenerateProcessor(string exportDirectory, OutputFile file)
        {
            bool hasAudioOutput = file.OutputSetting.selectedAudioOutputs.Count > 0;
            bool hasSubtitleOutput = file.OutputSetting.selectedSubtitleOutputs.Count > 0;
            bool isBurnSubtitle = file.OutputSetting.isBurnSubtitle;
            bool? isTextTypeSubtitle = hasSubtitleOutput ? file.OutputSetting.selectedSubtitleOutputs.First().isTextType : null;
            bool isSeek = file.EditInTime != TimeSpan.Zero;
            bool isSeekend = file.EditOutTime != file.DefaultOutTime;
            FFMpegArgumentProcessor ffmprocessor;

            FFMpegArguments ffmpegArgs = FFMpegArguments.FromFileInput(file.ParentFullName, true, options =>
            {
                options.WithArgument(new HideBannerArgument());
                if (isSeek)
                {
                    options.WithArgument(new SeekSecondArgument(file.EditInTime.TotalSeconds));
                    if (hasSubtitleOutput && isBurnSubtitle && isTextTypeSubtitle.HasValue && isTextTypeSubtitle.Value)
                    {
                        options.WithArgument(new CopyTimeSpanArgument());
                    }
                }
            });

            ffmprocessor = ffmpegArgs.OutputToFile(Path.Combine(exportDirectory, file.OutputFileName), true, options =>
            {
                // Burn text or image subtitle
                if (hasSubtitleOutput && isBurnSubtitle && isTextTypeSubtitle.HasValue && isTextTypeSubtitle.Value)
                {
                    if (isSeek)
                    {
                        options.WithArgument(new SeekSecondArgument(file.EditInTime.TotalSeconds));
                    }
                    SubtitleHardBurnOptions subtitleHardBurnOptions = SubtitleHardBurnOptions.Create(file.ParentFullName);
                    subtitleHardBurnOptions.SetSubtitleIndex(file.OutputSetting.selectedSubtitleOutputs.First().relativeStreamIndex);
                    options.WithVideoFilters(f => f.HardBurnSubtitle(subtitleHardBurnOptions));
                }
                else if (hasSubtitleOutput && isBurnSubtitle && isTextTypeSubtitle.HasValue && !isTextTypeSubtitle.Value)
                {
                    options.WithArgument(new CVF_OverlayArgument(file.OutputSetting.selectedSubtitleOutputs.First().relativeStreamIndex));
                }

                // Video setting
                options.WithArgument(new MapAllVideoTrackArgument());
                if (file.OutputSetting.videoSetting.selectedVideoCodec == "Copy")
                {
                    options.WithVideoCodec("copy");
                }
                else if (file.OutputSetting.videoSetting.selectedVideoCodec == "Libx264")
                {
                    options.WithVideoCodec("libx264");
                    options.WithSpeedPreset(file.OutputSetting.videoSetting.selectedSpeedPreset);
                    options.WithConstantRateFactor(file.OutputSetting.videoSetting.constantRateFactor);
                }
                else if (file.OutputSetting.videoSetting.selectedVideoCodec == "Libx265")
                {
                    options.WithVideoCodec("libx265");
                    options.WithSpeedPreset(file.OutputSetting.videoSetting.selectedSpeedPreset);
                    options.WithConstantRateFactor(file.OutputSetting.videoSetting.constantRateFactor);
                }

                // Audio setting
                if (hasAudioOutput)
                {
                    foreach (var audioOutput in file.OutputSetting.selectedAudioOutputs)
                    {
                        options.WithArgument(new MapAudioTrackArgument(audioOutput.relativeStreamIndex));
                    }
                    options.WithAudioCodec("copy");
                }

                // Output not burn text type subtitle
                if (hasSubtitleOutput && !isBurnSubtitle && isTextTypeSubtitle.HasValue && isTextTypeSubtitle.Value)
                {
                    foreach (var subtitleOutput in file.OutputSetting.selectedSubtitleOutputs)
                    {
                        options.WithArgument(new MapSubtitleTrackArgument(subtitleOutput.relativeStreamIndex));
                    }
                    options.WithArgument(new Mov_text_CodecArgument());
                }

                // Seek end
                if (isSeekend)
                {
                    TimeSpan duration = file.EditOutTime - file.EditInTime;
                    options.WithArgument(new EndSeekSecondArgument(duration.TotalSeconds));
                }
            });

            return ffmprocessor;
        }

        public static async Task ExecuteFFmpeg(string exportDirectory, List<OutputFile> outputFiles, Action<string> notifyProcessingFileName, Action<double> notifyProgressPercentage)
        {
            FFMpegArgumentProcessor processor;

            foreach (var file in outputFiles)
            {
                processor = GenerateProcessor(exportDirectory, file);

                processor.NotifyOnProgress(notifyProgressPercentage, file.Duration)
                         .CancellableThrough(out mCencelAction, 0);

                notifyProcessingFileName($"{file.OutputFileName}");
                notifyProgressPercentage(0.0);
                await processor.ProcessAsynchronously();
            }
        }
    }
}
