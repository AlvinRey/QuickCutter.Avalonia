using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

//using System.Management;

using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Arguments;

namespace QuickCutter_Avalonia.Handler
{
    internal class HideBannerArgument : IArgument
    {
        public string Text => "-hide_banner";
    }

    internal class ExportHandler
    {
        static public ExportInfo? ExportInfoInstance { get; set; }

        static public void ChangeExportDirectory(string exportDirectory)
        {
            ExportInfoInstance!.ExportDirectory = exportDirectory;
        }


        static public bool GenerateExportInfo(string exportDirectory, IList<OutputFile> outputFiles)
        {
            ExportInfoInstance = new ExportInfo()
            {
                ExportDirectory = exportDirectory,
                OutputFiles = new ObservableCollection<OutputFile>(outputFiles)
            };

            return ExportInfoInstance != null ? true : false;
        }

        static private FFMpegArgumentProcessor GenerateCopyArgsProcessor(OutputFile file)
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

                ffmprocessor = ffmpegArgs.OutputToFile(@$"{ExportInfoInstance!.ExportDirectory}\{file.OutputFileName}",
                                    true,
                                    options =>
                                    {
                                        options.WithVideoCodec("copy");
                                    });
            }
            else
            {
                FFMpegArguments ffmpegArgs = FFMpegArguments.FromFileInput(file.ParentFullName, true, options =>
                {
                    options.WithArgument(new HideBannerArgument());
                });
                ffmprocessor = ffmpegArgs.OutputToFile(@$"{ExportInfoInstance!.ExportDirectory}\{file.OutputFileName}",
                    true,
                    options =>
                    {
                        if (file.Edit_OutTime != TimeSpan.Zero)
                            options.EndSeek(file.Edit_OutTime);
                        options.WithVideoCodec("copy");
                    });
            }
            return ffmprocessor;
        }

        static private FFMpegArgumentProcessor GenerateTransCodeArgsProcessor(OutputFile file)
        {
            FFMpegArgumentProcessor ffmprocessor;
            FFMpegArguments ffmpegArgs = FFMpegArguments.FromFileInput(file.ParentFullName, true, options =>
            {
                options.WithArgument(new HideBannerArgument());
            });
            ffmprocessor = ffmpegArgs.OutputToFile(@$"{ExportInfoInstance!.ExportDirectory}\{file.OutputFileName}",
                    true,
                    options =>
                    {
                        options.WithVideoCodec(file.SelectedCodec);
                        options.WithSpeedPreset(file.SelectedSpeedPreset);
                        options.WithConstantRateFactor((int)file.ConstantRateFactor);
                        options.WithVideoFilters(videoFilterOptions =>
                        {
                            if (!file.UsingCustomResolution && file.SelectedResolution != VideoSize.Original)
                            {
                                videoFilterOptions.Scale(file.SelectedResolution);
                            }
                            else
                            {
                                videoFilterOptions.Scale(file.CustomWidth, file.CustomHeight);
                            }
                        });
                        if (file.Edit_InTime != TimeSpan.Zero)
                        {
                            options.Seek(file.Edit_InTime);
                        }
                        if (file.Edit_OutTime != TimeSpan.Zero)
                        {
                            options.EndSeek(file.Edit_OutTime);
                        }
                    });

            return ffmprocessor;
        }

        static public void ExecuteFFmpeg()
        {
            FFMpegArgumentProcessor processor;
            foreach (var file in ExportInfoInstance!.OutputFiles)
            {
                file.IsProcessing = true;

                if (file.IsTransCode)
                {
                    processor = GenerateTransCodeArgsProcessor(file);
                }
                else
                {
                    processor = GenerateCopyArgsProcessor(file);
                }

                processor.NotifyOnProgress(new Action<double>(p =>
                                                                {
                                                                    file.ProcessingPercent = p;
                                                                }), file.Duration)
                         .CancellableThrough(out file.cencelOutput, 0);


                Task task = Execute(processor);
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // 处理异常
                        FFMpegException? exception = t.Exception.InnerException as FFMpegException;
                        if (exception != null)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                    }
                    file.IsProcessing = false;
                }, TaskContinuationOptions.None);
            }
        }

        static private async Task Execute(FFMpegArgumentProcessor ffmpegArgsProcessor)
        {
            await ffmpegArgsProcessor.ProcessAsynchronously();
        }
    }
}
