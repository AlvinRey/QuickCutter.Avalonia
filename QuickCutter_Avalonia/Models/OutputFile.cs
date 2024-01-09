﻿using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

using QuickCutter_Avalonia.Handler;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace QuickCutter_Avalonia.Models
{
    public partial class OutputFile : ReactiveObject
    {
        public string ParentFullName { get; set; }

        public string OutputFileName { get; set; }

        public string OutputFileExt { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan Default_OutTime { get; }

        public int DefaultHeight { get; }

        public int DefaultWidth { get; }

        [Reactive]
        public TimeSpan? Edit_InTime{ get; set; }

        [Reactive]
        public TimeSpan? Edit_OutTime{ get; set; }

        [Reactive]
        public bool IsTransCode{ get; set; }

        [Reactive]
        public bool UsingCustonSetting{ get; set; }

        [Reactive]
        public double ProcessingPercent{ get; set; }

        [Reactive]
        public bool IsProcessing{ get; set; }

        public Action? cencelOutput;

        // FFmpeg Options
        public IEnumerable<Codec> CodecOptions { get; } = Utility.GetCodec();

        [Reactive]
        public Codec SelectedCodec { get; set; } = VideoCodec.LibX264;

        public IEnumerable<Speed> SpeedPresetOptions { get; } = Enum.GetValues(typeof(Speed)).Cast<Speed>();

        [Reactive]
        public Speed SelectedSpeedPreset { get; set; } = Speed.Medium;

        [Reactive]
        public int ConstantRateFactor { get; set; }

        public IEnumerable<VideoSize> ResolutionOptions { get; } = Enum.GetValues(typeof(VideoSize)).Cast<VideoSize>();

        [Reactive]
        public VideoSize SelectedResolution { get; set; }

        [Reactive]
        public bool UsingCustomResolution { get; set; }

        [Reactive]
        public int CustomWidth { get; set; }

        [Reactive]
        public int CustomHeight { get; set; }

        public OutputFile(VideoInfo parentVideoInfo)
        {
            ParentFullName = parentVideoInfo.VideoFullName!;
            //Duration = parentVideoInfo.AnalysisResult!.Duration;
            ProcessingPercent = 0.0;

            // File Name Setting
            //OutputFileExt = System.IO.Path.GetExtension(importFileFullName);
            OutputFileExt = ".mp4";
            string fileName = System.IO.Path.GetFileNameWithoutExtension(parentVideoInfo.VideoFullName!);
            OutputFileName = $"{fileName}_Output_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{OutputFileExt}";
            // Time Setting
            Edit_InTime = TimeSpan.Zero;
            Default_OutTime = parentVideoInfo.AnalysisResult!.Duration;
            Edit_OutTime = Default_OutTime;

            this.WhenAnyValue(v => v.Edit_InTime).Subscribe(v =>
            {
                if (v <= TimeSpan.Zero)
                    Edit_InTime = TimeSpan.Zero;
                if (v >=Edit_OutTime)
                    Edit_InTime = Edit_OutTime - TimeSpan.FromSeconds(1);
            });

            this.WhenAnyValue(v => v.Edit_OutTime).Subscribe(v =>
            {
                if (v <= Edit_InTime)
                    Edit_OutTime = Edit_InTime + TimeSpan.FromSeconds(1);
                if (v >= Default_OutTime)
                    Edit_OutTime = Default_OutTime;
            });
            // Resolution Setting
            SelectedResolution = VideoSize.Original;
            DefaultHeight = parentVideoInfo.AnalysisResult!.PrimaryVideoStream!.Height;
            DefaultWidth = parentVideoInfo.AnalysisResult!.PrimaryVideoStream!.Width;
            UsingCustomResolution = false;
            CustomWidth = DefaultWidth;
            CustomHeight = DefaultHeight;

            // CRF Setting
            ConstantRateFactor = 21;

            // Codec Setting
            SelectedCodec = VideoCodec.LibX264;
        }

        //partial void OnEdit_InTimeChanged(TimeSpan? value)
        //{
        //    TimeSpan rightTime = Edit_OutTime != null ? (TimeSpan)Edit_OutTime : Default_OutTime;
        //    if (value == null) // newValue == null
        //    {
        //        Duration = rightTime - Default_InTime;
        //        return;
        //    }
        //    // Left Limit
        //    if (value <= TimeSpan.Zero)
        //    {
        //        Edit_InTime = null;
        //        return;
        //    }
        //    // Right Limit
        //    if (value >= rightTime)
        //    {
        //        Edit_InTime = rightTime.Subtract(new TimeSpan(0, 0, 1));
        //        return;
        //    }
        //    Duration = rightTime - (TimeSpan)Edit_InTime!;
        //}

        //partial void OnEdit_OutTimeChanged(TimeSpan? value)
        //{
        //    TimeSpan leftTime = Edit_InTime != null ? (TimeSpan)Edit_InTime : Default_InTime;
        //    if (value == null) // newValue == null
        //    {
        //        Duration = Default_OutTime - leftTime;
        //        return;
        //    }

        //    // Left Limit
        //    if (value <= leftTime)
        //    {
        //        Edit_OutTime = leftTime.Add(new TimeSpan(0, 0, 1));
        //        return;
        //    }
        //    //Right Limit
        //    if (value >= Default_OutTime)
        //    {
        //        Edit_OutTime = null;
        //        return;
        //    }

        //    Duration = (TimeSpan)value - leftTime;
        //}

        //partial void OnSelectedResolutionChanged(VideoSize value)
        //{
        //    switch (value)
        //    {
        //        case VideoSize.Original:
        //            CustomWidth = DefaultWidth;
        //            CustomHeight = DefaultHeight;
        //            break;

        //        case VideoSize.FullHd:
        //            CustomWidth = 1920;
        //            CustomHeight = 1080;
        //            break;

        //        case VideoSize.Hd:
        //            CustomWidth = 1280;
        //            CustomHeight = 720;
        //            break;

        //        case VideoSize.Ed:
        //            CustomWidth = 720;
        //            CustomHeight = 480;
        //            break;

        //        case VideoSize.Ld:
        //            CustomWidth = 640;
        //            CustomHeight = 360;
        //            break;
        //    }
        //}

        //[RelayCommand]
        public void CencelOutput()
        {
            cencelOutput?.Invoke();
        }
    }
}
