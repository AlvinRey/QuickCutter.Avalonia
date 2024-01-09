using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Shared;


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
