using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
