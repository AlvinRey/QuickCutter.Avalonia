using FFMpegCore;

namespace QuickCutter_Avalonia.Models
{
    internal class FFmpegHandler
    {
        static public IMediaAnalysis AnaliysisMedia(string mediaFullName)
        {
            return FFProbe.Analyse(mediaFullName);
        }
    }
}