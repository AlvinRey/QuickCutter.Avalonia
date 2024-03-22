using FFMpegCore.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Models
{
    public struct AudioStreamOriginalInfo
    {
        public string Name { get; set; }

        public int absoluteStreamIndex;

        public int relativeStreamIndex;
    }
}
