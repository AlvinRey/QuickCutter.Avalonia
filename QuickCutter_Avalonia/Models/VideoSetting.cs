using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Models
{
    public struct VideoSetting
    {
        public string selectedVideoCodec;

        //H.264 AVOptions
        public Speed selectedSpeedPreset;
        public int constantRateFactor;
    }
}
