using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

using QuickCutter_Avalonia.Handler;

namespace QuickCutter_Avalonia.Models
{
    public partial class OutputFile : ObservableObject
    {
        public string ParentFullName { get; set; }

        public string OutputFileName { get; set; }

        public string OutputFileExt { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan Default_InTime { get; }

        public TimeSpan Default_OutTime { get; }

        public int DefaultHeight { get; }

        public int DefaultWidth { get; }

        [ObservableProperty]
        private TimeSpan? edit_InTime;

        [ObservableProperty]
        private TimeSpan? edit_OutTime;

        [ObservableProperty]
        private bool isTransCode;

        [ObservableProperty]
        private bool usingCustonSetting;

        [ObservableProperty]
        private double processingPercent;

        [ObservableProperty]
        private bool isProcessing;

        public Action? cencelOutput;

        // FFmpeg Options
        public IEnumerable<string> PlatfromPresetOptions { get; } = new[] { "BiliBili", "WeChat" };

        [ObservableProperty]
        private string platfromPreset = "BiliBili";

        public IEnumerable<FFMpegCore.Enums.Codec> CodecOptions { get; } = Utility.GetCodec();

        [ObservableProperty]
        private FFMpegCore.Enums.Codec selectedCodec = FFMpegCore.Enums.VideoCodec.LibX264;

        public IEnumerable<FFMpegCore.Enums.Speed> SpeedPresetOptions { get; } = Enum.GetValues(typeof(FFMpegCore.Enums.Speed)).Cast<FFMpegCore.Enums.Speed>();

        [ObservableProperty]
        private FFMpegCore.Enums.Speed selectedSpeedPreset = FFMpegCore.Enums.Speed.Medium;

        [ObservableProperty]
        private float constantRateFactor;

        public IEnumerable<FFMpegCore.Enums.VideoSize> ResolutionOptions { get; } = Enum.GetValues(typeof(FFMpegCore.Enums.VideoSize)).Cast<FFMpegCore.Enums.VideoSize>();

        [ObservableProperty]
        private FFMpegCore.Enums.VideoSize selectedResolution;

        [ObservableProperty]
        private bool usingCustomResolution;

        [ObservableProperty]
        private int customWidth;

        [ObservableProperty]
        private int customHeight;

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
            return;
            // Time Setting
            Default_InTime = TimeSpan.Zero;
            Edit_InTime = null;
            Default_OutTime = parentVideoInfo.AnalysisResult!.Duration;
            Edit_OutTime = null;

            // Resolution Setting
            SelectedResolution = FFMpegCore.Enums.VideoSize.Original;
            DefaultHeight = parentVideoInfo.AnalysisResult!.PrimaryVideoStream!.Height;
            DefaultWidth = parentVideoInfo.AnalysisResult!.PrimaryVideoStream!.Width;
            UsingCustomResolution = false;
            CustomWidth = DefaultWidth;
            CustomHeight = DefaultHeight;

            // CRF Setting
            ConstantRateFactor = 21;

            // Codec Setting
            SelectedCodec = FFMpegCore.Enums.VideoCodec.LibX264;
        }

        partial void OnEdit_InTimeChanged(TimeSpan? value)
        {
            TimeSpan rightTime = Edit_OutTime != null ? (TimeSpan)Edit_OutTime : Default_OutTime;
            if (value == null) // newValue == null
            {
                Duration = rightTime - Default_InTime;
                return;
            }
            // Left Limit
            if (value <= TimeSpan.Zero)
            {
                Edit_InTime = null;
                return;
            }
            // Right Limit
            if (value >= rightTime)
            {
                Edit_InTime = rightTime.Subtract(new TimeSpan(0, 0, 1));
                return;
            }
            Duration = rightTime - (TimeSpan)Edit_InTime!;
        }

        partial void OnEdit_OutTimeChanged(TimeSpan? value)
        {
            TimeSpan leftTime = Edit_InTime != null ? (TimeSpan)Edit_InTime : Default_InTime;
            if (value == null) // newValue == null
            {
                Duration = Default_OutTime - leftTime;
                return;
            }

            // Left Limit
            if (value <= leftTime)
            {
                Edit_OutTime = leftTime.Add(new TimeSpan(0, 0, 1));
                return;
            }
            //Right Limit
            if (value >= Default_OutTime)
            {
                Edit_OutTime = null;
                return;
            }

            Duration = (TimeSpan)value - leftTime;
        }

        partial void OnSelectedResolutionChanged(VideoSize value)
        {
            switch (value)
            {
                case VideoSize.Original:
                    CustomWidth = DefaultWidth;
                    CustomHeight = DefaultHeight;
                    break;

                case VideoSize.FullHd:
                    CustomWidth = 1920;
                    CustomHeight = 1080;
                    break;

                case VideoSize.Hd:
                    CustomWidth = 1280;
                    CustomHeight = 720;
                    break;

                case VideoSize.Ed:
                    CustomWidth = 720;
                    CustomHeight = 480;
                    break;

                case VideoSize.Ld:
                    CustomWidth = 640;
                    CustomHeight = 360;
                    break;
            }
        }

        [RelayCommand]
        private void CencelOutput()
        {
            cencelOutput?.Invoke();
        }
    }
}
