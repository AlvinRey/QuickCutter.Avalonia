using System.Collections.Generic;

namespace QuickCutter_Avalonia.Models
{
    public class OutputSetting
    {
        public bool isBurnSubtitle;
        public string key;
        public VideoSetting videoSetting;
        public List<AudioStreamOriginalInfo> selectedAudioOutputs;
        public List<SubtitleStreamOriginalInfo> selectedSubtitleOutputs;
    }
}
