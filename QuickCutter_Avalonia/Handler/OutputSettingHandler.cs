using DynamicData;
using FFMpegCore.Enums;
using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickCutter_Avalonia.Handler
{
    internal static class OutputSettingHandler
    {
        public static List<string> VideoCodecs => ["Copy", "Libx264", "Libx265"];
        //public static List<string> AudioCodecs => ["Copy", "aac", "ac3", "eac3", "mp3", "flac", "opus", "vorbis"];
        public static List<Speed> SpeedPresets = Enum.GetValues(typeof(Speed)).Cast<Speed>().ToList();
        public static Dictionary<string, List<AudioStreamOriginalInfo>> AudioStreamDictonary = new ();
        public static Dictionary<string, List<SubtitleStreamOriginalInfo>> SubtitleStreamDictonary = new ();

        public static OutputSetting GenerateOutputSetting(string parentFullName)
        {
            var outputSetting = new OutputSetting()
            {
                key = parentFullName,
                videoSetting = new VideoSetting()
                {
                    useNetworkOptimization = false,
                    selectedVideoCodec = VideoCodecs.First(),
                    selectedSpeedPreset = SpeedPresets[3],
                    constantRateFactor = 23
                },
                selectedAudioOutputs = new (),
                selectedSubtitleOutputs = new ()
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
