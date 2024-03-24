using DynamicData;
using FFMpegCore.Enums;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuickCutter_Avalonia.Models;
using QuickCutter_Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class OutputSettingHandler
    {
        static public List<string> VideoCodecs
            = new List<string>()
            {
                "Copy",
                "Libx264",
                "Libx265",
            };
        static public List<Speed> SpeedPresets = Enum.GetValues(typeof(Speed)).Cast<Speed>().ToList();
        public static Dictionary<string, List<AudioStreamOriginalInfo>> AudioStreamDictonary = new Dictionary<string, List<AudioStreamOriginalInfo>>();
        static public List<string> AudioCodecs
            = new List<string>()
            {
                "Copy",
                "aac",
                "ac3",
                "eac3",
                "mp3",
                "flac",
                "opus",
                "vorbis",
            };

        public static Dictionary<string, List<SubtitleStreamOriginalInfo>> SubtitleStreamDictonary = new Dictionary<string, List<SubtitleStreamOriginalInfo>>();


        public static OutputSetting GenerateOutputSetting(string parentFullName)
        {
            var outputSetting = new OutputSetting()
            {
                key = parentFullName,
                videoSetting = new VideoSetting()
                {
                    selectedVideoCodec = VideoCodecs.First(),
                    selectedSpeedPreset = SpeedPresets.First(),
                    constantRateFactor = 23
                },
                selectedAudioOutputs = new List<AudioStreamOriginalInfo>(),
                selectedSubtitleOutputs = new List<SubtitleStreamOriginalInfo>()
            };
            return outputSetting;
        }
    }


    static class OutputSettingExtensions
    {
        public static void CopyFrom(this OutputSetting target, OutputSetting source)
        {
            target.videoSetting.CopyFrom(source.videoSetting);
        }
    }

    static class VideoSettingExtensions
    {
        public static void CopyFrom(this VideoSetting target, VideoSetting source)
        {
            target.selectedVideoCodec = source.selectedVideoCodec;
            if (target.selectedVideoCodec == "libx264(H.264)")
            {
                target.selectedSpeedPreset = source.selectedSpeedPreset;
                target.constantRateFactor = source.constantRateFactor;
            }
        }
    }

    //static class AudioSettingExtensions
    //{
    //    public static void CopyFrom(this AudioSetting target, AudioSetting source)
    //    {

    //    }
    //}

    //static class SubtitleSettingExtensions
    //{
    //    public static void CopyFrom(this SubtitleSetting target, SubtitleSetting source)
    //    {

    //    }
    //}
}
