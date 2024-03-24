using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Models
{
    public struct SubtitleStreamOriginalInfo
    {
        public string name;

        public int absoluteStreamIndex;

        public int relativeStreamIndex;

        public bool isTextType;
    }
}
