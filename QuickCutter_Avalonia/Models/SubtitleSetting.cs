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
        public string Name { get; set; }

        public int absoluteStreamIndex;

        public int relativeStreamIndex;

        public bool IsTextType { get; set; }
    }
}
