﻿using Avalonia;
using FFMpegCore.Enums;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
namespace QuickCutter_Avalonia.Mode
{
    public partial class OutputFile : ReactiveObject, IDisposable
    {
        #region Private Member
        private CompositeDisposable _subscriptions;
        #endregion

        public IReactiveCommand ReplayCommand { get; }

        [Reactive]
        public int RowIndex { get; set; }

        public string ParentFullName { get; set; }

        public string OutputFileName { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan Default_OutTime { get; }

        public int DefaultHeight { get; }

        public int DefaultWidth { get; }

        public List<string> OutputFileExtOptions { get; set; }

        [Reactive]
        public string OutputFileExt { get; set; }

        [Reactive]
        public TimeSpan Edit_InTime { get; set; }

        [Reactive]
        public TimeSpan Edit_OutTime { get; set; }

        [Reactive]
        public bool IsTransCode { get; set; }

        public Action? cencelOutput;

        // Stream
        public ObservableCollection<StreamInfo> AudioStreams { get; set; }
        public IReactiveCommand SelectAllAudioCommand { get; }
        public IReactiveCommand CencelSelectAllAudioCommand { get; }

        public ObservableCollection<StreamInfo> SubtitleStreams { get; set; }
        public IReactiveCommand SelectAllSubtitleCommand { get; }
        public IReactiveCommand CencelSelectAllSubtitleCommand { get; }

        // FFmpeg Options
        public IEnumerable<Codec> CodecOptions { get; } = Utils.GetCodec();

        [Reactive]
        public Codec SelectedCodec { get; set; } = VideoCodec.LibX264;

        [Reactive]
        public int CustomWidth { get; set; }

        [Reactive]
        public bool IsLinkResolution { get; set; } = true;

        [Reactive]
        public int CustomHeight { get; set; }

        public List<Size> ResolutionPreset { get; set; }


        // Libx264 AvOption
        public IEnumerable<Speed> SpeedPresetOptions { get; } = Enum.GetValues(typeof(Speed)).Cast<Speed>();

        [Reactive]
        public Speed SelectedSpeedPreset { get; set; } = Speed.Medium;

        [Reactive]
        public int? ConstantRateFactor { get; set; }



        public OutputFile(VideoInfo parentVideoInfo)
        {
            ParentFullName = parentVideoInfo.VideoFullName!;
            //Duration = parentVideoInfo.AnalysisResult!.Duration;

            // File Name Setting
            string fileName = System.IO.Path.GetFileNameWithoutExtension(parentVideoInfo.VideoFullName!);
            OutputFileName = $"{fileName}_Output_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            OutputFileExtOptions = new List<string>
            {
                new string(".mp4"),
                new string(".mov")
            };
            OutputFileExt = OutputFileExtOptions.First();

            // Time Setting
            Edit_InTime = TimeSpan.Zero;
            Default_OutTime = parentVideoInfo.AnalysisResult!.Duration;
            Edit_OutTime = Default_OutTime;

            var edit_InTimeChanged = this.WhenAnyValue(v => v.Edit_InTime);
            var edit_OutTimeChanged = this.WhenAnyValue(v => v.Edit_OutTime);

            // Streams Setting
            AudioStreams = new ObservableCollection<StreamInfo>();
            SubtitleStreams = new ObservableCollection<StreamInfo>();
            int index = 0;
            foreach (var audio in parentVideoInfo.AnalysisResult.AudioStreams)
            {
                AudioStreams.Add(new StreamInfo() { Name = FFmpegHandler.GetStreamName(audio, ++index), AbsoluteStreamIndex = audio.Index });
            }
            index = 0;
            foreach (var subtitle in parentVideoInfo.AnalysisResult.SubtitleStreams)
            {
                SubtitleStreams.Add(new StreamInfo() { Name = FFmpegHandler.GetStreamName(subtitle, ++index), AbsoluteStreamIndex = subtitle.Index });
            }

            // Resolution Setting
            DefaultHeight = parentVideoInfo.AnalysisResult!.PrimaryVideoStream!.Height;
            DefaultWidth = parentVideoInfo.AnalysisResult!.PrimaryVideoStream!.Width;
            CustomWidth = DefaultWidth;
            CustomHeight = DefaultHeight;
            var customWidthChanged = this.WhenAnyValue(v => v.CustomWidth);
            var customHeightChanged = this.WhenAnyValue(v => v.CustomHeight);

            // CRF Setting
            ConstantRateFactor = 23;

            // Codec Setting
            SelectedCodec = VideoCodec.LibX264;

            ResolutionPreset = new List<Size>
            {
                new Size(3840, 2160),
                new Size(2560, 1440),
                new Size(1920, 1080),
                new Size(1280, 720),
                new Size(640, 480)
            };

            _subscriptions = new CompositeDisposable
            {
                edit_InTimeChanged.Subscribe(v =>
                    {
                        if (v <= TimeSpan.Zero)
                            Edit_InTime = TimeSpan.Zero;
                        if (v >= Edit_OutTime)
                            Edit_InTime = Edit_OutTime - TimeSpan.FromSeconds(1);

                        Duration = (TimeSpan)(Edit_OutTime - Edit_InTime);
                    }),
                edit_OutTimeChanged.Subscribe(v =>
                    {
                        if (v <= Edit_InTime)
                            Edit_OutTime = Edit_InTime + TimeSpan.FromSeconds(1);
                        if (v >= Default_OutTime)
                            Edit_OutTime = Default_OutTime;
                        Duration = (TimeSpan)(Edit_OutTime - Edit_InTime);
                    }),
                customWidthChanged.Subscribe(v =>
                {
                    if(IsLinkResolution)
                    {
                        var res = ResolutionPreset.FirstOrDefault(v => v.Width == CustomWidth);
                        if(res != new Size(0,0) && CustomHeight != res.Height)
                            CustomHeight = (int)res.Height;
                    }
                }),
                customHeightChanged.Subscribe(v =>
                {
                    if(IsLinkResolution)
                    {
                        var res = ResolutionPreset.FirstOrDefault(v => v.Height == CustomHeight);
                        if(res != new Size(0,0) && CustomWidth != res.Width)
                            CustomWidth = (int)res.Width;
                    }
                }),
            };

            ReplayCommand = ReactiveCommand.Create(() => MediaPlayerHandler.Replay((long)Edit_InTime.TotalMilliseconds, (long)Edit_OutTime.TotalMilliseconds));

            SelectAllAudioCommand = ReactiveCommand.Create(() => 
            {
                foreach(var streamInfo in AudioStreams)
                {
                    streamInfo.Mapped = true;
                }
            });

            CencelSelectAllAudioCommand = ReactiveCommand.Create(() =>
            {
                foreach (var streamInfo in AudioStreams)
                {
                    streamInfo.Mapped = false;
                }
            });

            SelectAllSubtitleCommand = ReactiveCommand.Create(() =>
            {
                foreach (var streamInfo in SubtitleStreams)
                {
                    streamInfo.Mapped = true;
                }
            });

            CencelSelectAllSubtitleCommand = ReactiveCommand.Create(() =>
            {
                foreach (var streamInfo in SubtitleStreams)
                {
                    streamInfo.Mapped = false;
                }
            });
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _subscriptions = null;
        }
    }
}
