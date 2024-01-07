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
using Avalonia;
using Avalonia.Controls.Primitives;




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
            Projects = new ObservableCollection<Project>();
            MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.EndReached += MediaPlayer_EndReached;
            MediaPlayer.Stopped += MediaPlayer_Stopped;
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
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

        public IEnumerable<TrackDescription> AudioTrack
        {
            get
            {
                Debug.WriteLine("UI Get AudioTrackDescription");
                return MediaPlayer.AudioTrackDescription.AsEnumerable();
            }
        }
        [ObservableProperty]
        private TrackDescription? selectedAudioTrack;

        public IEnumerable<TrackDescription> SubtitleTrack
        {
            get
            {
                Debug.WriteLine("UI Get SpuDescription");
                return MediaPlayer.SpuDescription.AsEnumerable();
            }
        }
        [ObservableProperty]
        private TrackDescription? selectedSubtitleTrack;

        public bool IsPlaying
        {
            get => MediaPlayer.IsPlaying;
        }

        private void MediaPlayer_PositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Position));
        }
        private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CurrentTime));
        }
        private void MediaPlayer_LengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Duration));
        }
        private void MediaPlayer_VolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e)
        {
            Debug.WriteLine(e.Volume);
            OnPropertyChanged(nameof(Volume));
        }
        private void MediaPlayer_Playing(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsPlaying));
            NotifyTrackOptionsChange();
            OnPropertyChanged(nameof(Volume));
        }
        private void MediaPlayer_Paused(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsPlaying));
        }
        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsPlaying));
        }
        private void MediaPlayer_Stopped(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsPlaying));
        }
        public void NotifyTrackOptionsChange()
        {
            OnPropertyChanged(nameof(AudioTrack));
            OnPropertyChanged(nameof(SubtitleTrack));
        }

        public void SelectCurrentAudioTrack()
        {
            foreach (var track in MediaPlayer.AudioTrackDescription)
            {
                if (track.Id == MediaPlayer.AudioTrack)
                {
                    SelectedAudioTrack = track;
                    break;
                }
            }
        }

        public void SelectCurrentSubtitleTrack()
        {
            foreach (var track in MediaPlayer.SpuDescription)
            {
                if (track.Id == MediaPlayer.Spu)
                {
                    SelectedSubtitleTrack = track;
                    break;
                }
            }
        }

        public void LoadMedia()
        {
            var media = new Media(_libVlc, new Uri(SelectedProject!.ImportVideoInfo.VideoFullName!));
            MediaPlayer.Media = media;

            MediaPlayer.Play();
        }

        public void UnLoadMdeia()
        {
            if (MediaPlayer.IsPlaying)
                MediaPlayer.Stop();
        }

        partial void OnSelectedAudioTrackChanged(TrackDescription? value)
        {
            if (value.HasValue)
            {
                MediaPlayer.SetAudioTrack(value.Value.Id);
            }
        }

        partial void OnSelectedSubtitleTrackChanged(TrackDescription? value)
        {
            if (value.HasValue)
            {
                MediaPlayer.SetSpu(value.Value.Id);
            }
        }

        [RelayCommand]
        private void PlayOrPauseVideo()
        {
            switch(MediaPlayer.State)
            {
                case VLCState.Playing:
                case VLCState.Paused:
                    MediaPlayer.Pause();
                    break;
                case VLCState.Stopped:
                case VLCState.Ended:
                    MediaPlayer.Play(MediaPlayer.Media!);
                    break;
            }
        }

        public void ImportProjectFile(VideoInfo videoInfo)
        {
            Projects.Add(new Project(videoInfo));
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
