using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using CommunityToolkit.Mvvm.Input;

namespace QuickCutter_Avalonia.Models
{
    public partial class Project : ObservableObject
    {
        public VideoInfo ImportVideoInfo { get; set; }

        public ObservableCollection<OutputFile> OutputFiles { get; set; }

        public Project(VideoInfo videoInfo)
        {
            ImportVideoInfo = videoInfo;
            OutputFiles = new ObservableCollection<OutputFile>() { new OutputFile(ImportVideoInfo) };
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
