using System.Collections.ObjectModel;


namespace QuickCutter_Avalonia.Models
{
    public partial class Project
    {
        public VideoInfo ImportVideoInfo { get; set; }

        public ObservableCollection<OutputFile> OutputFiles { get; set; }

        public Project(VideoInfo videoInfo)
        {
            ImportVideoInfo = videoInfo;
            OutputFiles = new ObservableCollection<OutputFile>();
            OutputFiles.CollectionChanged += OutputFiles_CollectionChanged;
        }

        private void OutputFiles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var i = 0; i < OutputFiles.Count; i++)
            {
                OutputFiles[i].RowIndex = i + 1;
            }
        }

        public void AddChild()
        {
            OutputFiles.Add(new OutputFile(ImportVideoInfo));
        }

        public bool HasChild(ref OutputFile item)
        {
            return OutputFiles.Contains(item);
        }
    }
}
