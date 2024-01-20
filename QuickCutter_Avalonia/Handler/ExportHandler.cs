using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Exceptions;

using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ursa.Controls;

namespace QuickCutter_Avalonia.Handler
{
    internal class HideBannerArgument : IArgument
    {
        public string Text => "-hide_banner";
    }

    internal class ExportHandler
    {
        static private bool mIsCencel = false;
        static private Action? mCencelAction;
        static public ExportInfo? ExportInfoInstance { get; set; }

        static public void Setup()
        {
            if(File.Exists(Path.Combine(Utility.StartupPath(), @"bin\ffmpeg.exe")) && File.Exists(Path.Combine(Utility.StartupPath(), @"bin\ffprobe.exe")))
            {
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine(Utility.StartupPath(),"bin"), TemporaryFilesFolder = Path.Combine(Utility.StartupPath(), "temp") });
                return;
            }

            string? pathVariable = Environment.GetEnvironmentVariable("PATH");

            // must ToList(), in order to Linq Once.
            var paths = pathVariable?.Split(';').Where(path => path.Contains("ffmpeg"));
            bool containsFFmpeg = false;

            if (paths != null)
            {
                foreach (var path in paths)
                {
                    containsFFmpeg = File.Exists(Path.Combine(path, "ffmpeg.exe")) && File.Exists(Path.Combine(path, "ffprobe.exe"));
                    if(containsFFmpeg)
                    {
                        break;
                    }
                }
            }

            if(!containsFFmpeg)
            {
                MessageBox.ShowAsync("Can not find FFmpeg in PATH, Plese download FFmpeg or add FFmpeg into PATH", "Error", MessageBoxIcon.Error, MessageBoxButton.OK);
            }
        }

        static public void ChangeExportDirectory(string exportDirectory)
        {
            ExportInfoInstance!.ExportDirectory = exportDirectory;
        }

        static public void CencelWithAppQuit()
        {
            if(mCencelAction != null)
            {
                CencelExport();
            }
        }

        static public void CencelExport()
        {
            mIsCencel = true;
            mCencelAction?.Invoke();
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
                        options.WithVideoFilters(videoFilterOptions => videoFilterOptions.Scale(file.CustomWidth, file.CustomHeight));
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

        async static public Task ExecuteFFmpeg()
        {
            FFMpegArgumentProcessor processor;
            foreach (var file in ExportInfoInstance!.OutputFiles)
            {
                file.IsReady = true;
            }

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
                                                                    Debug.WriteLine(file.ProcessingPercent);
                                                                }), file.Duration)
                         .CancellableThrough(out mCencelAction, 0);

                if (mIsCencel)
                    break;

                try
                {
                    await processor.ProcessAsynchronously();
                }
                catch (Exception ex)
                {
                    // 处理异常
                    FFMpegException? exception = ex as FFMpegException;
                    if (exception != null)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }
                finally
                {
                    file.IsProcessing = false;
                    file.IsReady = false;
                }
            }

            if(mIsCencel)
            {
                foreach (var file in ExportInfoInstance!.OutputFiles)
                {
                    file.IsProcessing = false;
                    file.IsReady = false;
                }
                mIsCencel = false;
            }
            mCencelAction = null;
        }
    }
}
