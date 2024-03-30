using FFMpegCore;

namespace QuickCutter_Avalonia.Models
{
    public class VideoInfo
    {
        public string VideoFullName { get; set; }

        public IMediaAnalysis AnalysisResult { get; set; }
    }
}
