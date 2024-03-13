using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Exceptions;

using QuickCutter_Avalonia.Mode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class HideBannerArgument : IArgument
    {
        public string Text => "-hide_banner";
    }

    internal class ExportHandler
    {
        static private Action? mCencelAction;

        static public void CencelExport()
        {
            mCencelAction?.Invoke();
        }

        static private FFMpegArgumentProcessor GenerateCopyArgsProcessor(string exportDirectory, OutputFile file)
        {
            FFMpegArgumentProcessor ffmprocessor;
            if (file.Edit_InTime != TimeSpan.Zero)
            {
                FFMpegArguments ffmpegArgs = FFMpegArguments.FromFileInput(file.ParentFullName, true, options =>
                {
                    options.WithArgument(new HideBannerArgument());
                    options.Seek(file.Edit_InTime);
                    if (file.Edit_OutTime != TimeSpan.Zero)
                        options.EndSeek(file.Edit_OutTime);
                });

                ffmprocessor = ffmpegArgs.OutputToFile(@$"{exportDirectory}\{file.OutputFileName}{file.OutputFileExt}",
                                    true,
                                    options =>
                                    {
                                        options.WithVideoCodec("copy")
                                               .WithAudioCodec("copy");
                                    });
            }
            else
            {
                FFMpegArguments ffmpegArgs = FFMpegArguments.FromFileInput(file.ParentFullName, true, options =>
                {
                    options.WithArgument(new HideBannerArgument());
                });
                ffmprocessor = ffmpegArgs.OutputToFile(@$"{exportDirectory}\{file.OutputFileName}{file.OutputFileExt}",
                    true,
                    options =>
                    {
                        if (file.Edit_OutTime != TimeSpan.Zero)
                            options.EndSeek(file.Edit_OutTime);
                        options.WithVideoCodec("copy")
                               .WithAudioCodec("copy");
                    });
            }
            return ffmprocessor;
        }

        static private FFMpegArgumentProcessor GenerateTransCodeArgsProcessor(string exportDirectory, OutputFile file)
        {
            FFMpegArgumentProcessor ffmprocessor;
            FFMpegArguments ffmpegArgs = FFMpegArguments.FromFileInput(file.ParentFullName, true, options =>
            {
                options.WithArgument(new HideBannerArgument());
                options.Seek(file.Edit_InTime);
                options.EndSeek(file.Edit_OutTime);
            });
            ffmprocessor = ffmpegArgs.OutputToFile(@$"{exportDirectory}\{file.OutputFileName}{file.OutputFileExt}",
                    true,
                    options =>
                    {
                        options.WithVideoCodec(file.SelectedCodec);
                        options.WithSpeedPreset(file.SelectedSpeedPreset);
                        options.WithConstantRateFactor(file.ConstantRateFactor != null ? (int)file.ConstantRateFactor : 23);
                        options.WithVideoFilters(videoFilterOptions => videoFilterOptions.Scale(file.CustomWidth, file.CustomHeight));
                        options.WithAudioCodec("copy");
                    });

            return ffmprocessor;
        }

        async static public Task ExecuteFFmpeg(string exportDirectory, IList<OutputFile> outputFiles, Action<string> notifyProcessingFileName, Action<double> notifyProgressPercentage)
        {
            FFMpegArgumentProcessor processor;

            foreach (var file in outputFiles)
            {
                if (file.IsTransCode)
                {
                    processor = GenerateTransCodeArgsProcessor(exportDirectory, file);
                }
                else
                {
                    processor = GenerateCopyArgsProcessor(exportDirectory, file);
                }

                processor.NotifyOnProgress(notifyProgressPercentage, file.Duration)
                         .CancellableThrough(out mCencelAction, 0);

                notifyProcessingFileName($"{file.OutputFileName}{file.OutputFileExt}");
                await processor.ProcessAsynchronously();
            }
        }
    }
}
