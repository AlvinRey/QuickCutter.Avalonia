using FFMpegCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;


namespace QuickCutter_Avalonia.Mode
{
    public partial class Project : ReactiveObject
    {
        public VideoInfo ImportVideoInfo { get; set; }

        [Reactive]
        public string MediaFullName { get; set; }

        public ObservableCollection<OutputFile> OutputFiles { get; set; }

        [Reactive]
        public bool Loading { get; private set; }

        public Project()
        {
            MediaFullName = string.Empty;
            OutputFiles = new ObservableCollection<OutputFile>();
            OutputFiles.CollectionChanged += OutputFiles_CollectionChanged;
            Loading = true;
        }

        public Project(VideoInfo videoInfo)
        {
            ImportVideoInfo = videoInfo;
            MediaFullName = ImportVideoInfo.VideoFullName;
            OutputFiles = new ObservableCollection<OutputFile>();
            OutputFiles.CollectionChanged += OutputFiles_CollectionChanged;
            Loading = false;
        }

        private void OutputFiles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var i = 0; i < OutputFiles.Count; i++)
            {
                OutputFiles[i].RowIndex = i + 1;
            }
        }

        public void SetVideoInfo(VideoInfo videoInfo)
        {
            ImportVideoInfo = videoInfo;
            MediaFullName = ImportVideoInfo.VideoFullName;
            Loading = false;
        }

        public void AddChild()
        {
            OutputFiles.Add(new OutputFile(ImportVideoInfo));
        }

        public OutputFile GetLastChild()
        {
            return OutputFiles.Last();
        }

        public bool HasChild(ref OutputFile item)
        {
            return OutputFiles.Contains(item);
        }
    }
}
