using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFMpegCore;

namespace QuickCutter_Avalonia.Models
{
    public class VideoInfo
    {
        public string VideoFullName { get; set; }

        public IMediaAnalysis AnalysisResult { get; set; }
    }
}
