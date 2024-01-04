using System.Windows;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using QuickCutter_Avalonia.Models;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Views;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using System;
using System.Threading;
using LibVLCSharp.Shared.Structures;
using System.Collections.Generic;
using Avalonia.Media.TextFormatting;
using System.Linq;




namespace QuickCutter_Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public static int projectNum = 0;
        public static int outputFileNum = 0;

        [ObservableProperty]
        private Project? selectedProject;

        [ObservableProperty]
        private OutputFile? selectedOutputFile;

        public ObservableCollection<Project> Projects { get; }

        public MainWindowViewModel()
        {
            MediaPlayer = new MediaPlayer(_libVlc);
            Projects = new ObservableCollection<Project>
            {
                //new Project(new VideoInfo{ VideoFullName = "Test_1.mp4"}),
                //new Project(new VideoInfo{ VideoFullName = "Test_2.mp4"})
            };
            MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            MediaPlayer.Playing += PeojectMediaPlayer_Playing;
            MediaPlayer.Paused += PeojectMediaPlayer_Paused;
            MediaPlayer.EndReached += MediaPlayer_EndReached;
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
        }



        [RelayCommand]
        private async Task ImportProjectFile()
        {
            VideoInfo? videoInfo = await FileHandler.ImportVideoFile();
            if (null != videoInfo)
            {
                Projects.Add(new Project(videoInfo));
            }
        }

        // media player setting
        private readonly LibVLC _libVlc = new LibVLC();

        public MediaPlayer MediaPlayer { get; }

        public TimeSpan CurrentTime
        {
            get => TimeSpan.FromMilliseconds(MediaPlayer.Time > -1 ? MediaPlayer.Time : 0);
        }

        public TimeSpan Duration => TimeSpan.FromMilliseconds(MediaPlayer.Length > -1 ? MediaPlayer.Length : 0);

        public float Position
        {
            get => MediaPlayer.Position;
            set
            {
                MediaPlayer.Position = value;
            }
        }

        public int Volume
        {
            get => MediaPlayer.Volume;
            set
            {
                MediaPlayer.Volume = value;
            }
        }

        private ObservableCollection<TrackDescription> ret = new ObservableCollection<TrackDescription>();
        public ObservableCollection<TrackDescription> AudioTrack
        {
            get
            {
                ret.Clear();
                foreach(var i in MediaPlayer.AudioTrackDescription)
                {
                    ret.Add(i);
                }
                return ret;
            }
        }
        [ObservableProperty]
        private TrackDescription selectedAudioTrack;

        public IEnumerable<TrackDescription> SubtitleTrack
        {
            get => MediaPlayer.SpuDescription.AsEnumerable();
        }
        [ObservableProperty]
        private TrackDescription selectedSubtitleTrack;

        [ObservableProperty]
        private bool isPlaying = false;
        private bool isReachEnd = false;

        [ObservableProperty]
        private bool testBool = false;

        private void MediaPlayer_PositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            OnPropertyChanged("Position");
        }
        private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            OnPropertyChanged("CurrentTime");
        }
        private void MediaPlayer_LengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
        {
            OnPropertyChanged("Duration");
        }
        private void MediaPlayer_VolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e)
        {
            OnPropertyChanged("Volume");
        }
        private void PeojectMediaPlayer_Playing(object? sender, EventArgs e)
        {
            IsPlaying = true;
            isReachEnd = false;
            //Debug.WriteLine("AudioTrack: {0}", AudioTrack.GetHashCode());
            //if (TestBool)
            //{
            //    OnPropertyChanged("AudioTrack");
            //    foreach (var track in AudioTrack)
            //    {
            //        if (track.Id == MediaPlayer.AudioTrack)
            //            SelectedAudioTrack = track;
            //    }
            //}
            //Debug.WriteLine("SubtitleTrack: {0}", SubtitleTrack.GetHashCode());
            //if (TestBool)
            //{
            //    OnPropertyChanged("SubtitleTrack");
            //    foreach (var track in SubtitleTrack)
            //    {
            //        if (track.Id == MediaPlayer.Spu)
            //            SelectedSubtitleTrack = track;
            //    }
            //}
        }
        private void PeojectMediaPlayer_Paused(object? sender, EventArgs e)
        {
            IsPlaying = false;
        }
        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            isReachEnd = true;
        }

        public void LoadMedia()
        {
            var media = new Media(_libVlc, new Uri(SelectedProject!.ImportVideoInfo.VideoFullName!));
            MediaPlayer.Media = media;

            MediaPlayer.Play();
            var tracks = media.Tracks;
        }
        public void UnLoadMdeia()
        {
            if (MediaPlayer.IsPlaying)
                MediaPlayer.Stop();
            MediaPlayer.Media = null;
        }

        partial void OnSelectedAudioTrackChanged(TrackDescription value)
        {
            MediaPlayer.SetAudioTrack(value.Id);
        }

        partial void OnSelectedSubtitleTrackChanged(TrackDescription value)
        {
            MediaPlayer.SetSpu(value.Id);
        }

        [RelayCommand]
        private void PlayOrPauseVideo()
        {
            if (!isReachEnd)
            {
                MediaPlayer.Pause();
            }
            else
            {
                MediaPlayer.Play(MediaPlayer.Media!);
                isReachEnd = false;
            }
        }

        [RelayCommand]
        private void Test()
        {
            Debug.WriteLine("AudioTrack: {0}", AudioTrack.GetHashCode());
            Debug.WriteLine("SubtitleTrack: {0}", SubtitleTrack.GetHashCode());

            OnPropertyChanged("AudioTrack");
            if (TestBool) // 一直都是True
            {
                foreach (var track in AudioTrack)
                {
                    if (track.Id == MediaPlayer.AudioTrack)
                        SelectedAudioTrack = track;
                }
            }
            OnPropertyChanged("SubtitleTrack");
            if (TestBool)
            {
                foreach (var track in SubtitleTrack)
                {
                    if (track.Id == MediaPlayer.Spu)
                        SelectedSubtitleTrack = track;
                }
            }
        }

        public void Dispose()
        {
            MediaPlayer?.Dispose();
            _libVlc?.Dispose();
        }



        //[RelayCommand]
        //public void DeleteProject()
        //{
        //    if (SelectedProject != null)
        //    {
        //        if(SelectedProject.outputFiles.Count > 0)
        //        {
        //            MessageBoxResult result = MessageBox.Show($"确定删除项目：{SelectedProject.ImportVideoInfo.VideoFullName}", "提示", MessageBoxButton.YesNo);
        //            if(result == MessageBoxResult.No)
        //            {
        //                return;
        //            }
        //        }
        //        OutputFile? temp = SelectedOutputFile;
        //        if (temp != null && SelectedProject.HasChild(ref temp))
        //        {
        //            SelectedOutputFile = null;
        //        }
        //        Projects.Remove(SelectedProject);
        //        SelectedProject = null;
        //    }
        //}

        //[RelayCommand]
        //public void DeleteOutputFile()
        //{
        //    if (SelectedProject != null && SelectedOutputFile != null)
        //    {
        //        OutputFile temp = SelectedOutputFile;
        //        if (SelectedProject.HasChild(ref temp))
        //        {
        //            SelectedProject.outputFiles.Remove(temp);
        //            SelectedOutputFile = null;
        //        }
        //    }

        //}
        //[RelayCommand]
        //public void AddProjectOutputFile()
        //{
        //    if (SelectedProject != null)
        //    {
        //        SelectedProject.AddChild();
        //    }
        //}

        //[RelayCommand]
        //public void ExprotSelected()
        //{
        //    if (SelectedOutputFile == null)
        //    {
        //        MessageBox.Show("当前未选中任何输出。");
        //        return;
        //    }
        //    string OutputPath = FilesHandler.SelectSaveFolder();
        //    if (!string.IsNullOrEmpty(OutputPath))
        //    {
        //        IList<OutputFile> files = new List<OutputFile>
        //        {
        //            SelectedOutputFile
        //        };
        //        if (ExportHandler.GenerateExportInfo(OutputPath, files))
        //        {
        //            bool? ret = (new ExportWindowView()).ShowDialog();
        //            if (ret == true)
        //            {
        //                ExportHandler.ExecuteFFmpeg();
        //            }
        //        }
        //    }
        //}
    }
}
