﻿using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace QuickCutter_Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private Config _config;
        #region Project List
        public ObservableCollection<Project> SelectedProject { get; set; }
        public ObservableCollection<Project> Projects { get; }
        public IReactiveCommand ImportProjectFileCommand { get; }
        #endregion

        #region Media Player
        private readonly LibVLC _libVlc;
        public MediaPlayer MediaPlayer { get; }
        private CompositeDisposable _subscriptions;
        private Subject<Unit> refresh;

        public IReactiveCommand PlayOrPauseVideoCommand { get; }
        public IReactiveCommand ForwardCommand { get; }
        public IReactiveCommand BackwardCommand { get; }
        public IReactiveCommand NextFrameCommand { get; }


        public TimeSpan CurrentTime
        {
            get => TimeSpan.FromMilliseconds(MediaPlayer.Time > -1 ? MediaPlayer.Time : 0);
        }

        public TimeSpan Duration => TimeSpan.FromMilliseconds(MediaPlayer.Length > -1 ? MediaPlayer.Length : 0);

        private float InteralPosition;
        public float Position
        {
            get
            {
                if(MediaPlayer.IsPlaying)
                    InteralPosition = MediaPlayer.Position;
                return InteralPosition;
            }
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
        [Reactive]
        public TrackDescription? SelectedAudioTrack { get; set; }

        public IEnumerable<TrackDescription> SubtitleTrack
        {
            get
            {
                Debug.WriteLine("UI Get SpuDescription");
                return MediaPlayer.SpuDescription.AsEnumerable();
            }
        }
        [Reactive]
        public TrackDescription? SelectedSubtitleTrack { get; set; }

        public bool IsPlaying
        {
            get => MediaPlayer.IsPlaying;
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
            var media = new Media(_libVlc, new Uri(SelectedProject.First().ImportVideoInfo.VideoFullName!));
            MediaPlayer.Media = media;
        }

        public void ResetMediaPlayer()
        {
            MediaPlayer.Pause();
            MediaPlayer.Stop();
            MediaPlayer.Media = null;
            refresh.OnNext(Unit.Default);
        }
        #endregion

        #region Output Data Grid
        [Reactive]
        public bool IsExporting { get; set; }
        public ObservableCollection<OutputFile> SelectedOutputFiles { get; set; }
        [Reactive]
        public OutputFile SelectedSingleOutputFile { get; set; }
        public IReactiveCommand<Unit, Unit> AddOutputFilesCommand { get; }
        public IReactiveCommand ExportCommand { get; }
        public IReactiveCommand CencelCommand { get; }
        #endregion

        public MainWindowViewModel()
        {
            _config = Utils.GetConfig();

            #region Init Project List
            Projects = new ObservableCollection<Project>();
            SelectedProject = new ObservableCollection<Project>();
            ImportProjectFileCommand = ReactiveCommand.Create(
                async () =>
                {
                    var filesFullName = await FileHandler.SelectFiles(FileHandler.SelectAllVideo);
                    int startIndex = Projects.Count;
                    for (int i = 0; i < filesFullName.Count; i++)
                    {
                        Projects.Add(new Project());
                    }
                    for (int i = 0; i < filesFullName.Count; i++)
                    {
                        VideoInfo videoInfo = new VideoInfo()
                        {   
                            VideoFullName = filesFullName[i], 
                            AnalysisResult = await FFmpegHandler.AnaliysisMedia(filesFullName[i]) 
                        };
                        Projects[startIndex + i].SetVideoInfo(videoInfo); 
                    }
                });
            #endregion

            #region Init Media Player
            _libVlc = new LibVLC();
            MediaPlayer = new MediaPlayer(_libVlc);
            refresh = new Subject<Unit>();
            bool operationActive = false;


            IObservable<Unit> Wrap(IObservable<Unit> source)
                => source.Where(_ => !operationActive).Merge(refresh).ObserveOn(AvaloniaScheduler.Instance);

            IObservable<Unit> VLCEvent(string name)
                => Observable.FromEventPattern(MediaPlayer, name).Select(_ => Unit.Default);

            void Operating(Action action)
            {
                operationActive = true;
                action();
                operationActive = false;
                refresh.OnNext(Unit.Default);
            }
            var positionChanged = VLCEvent(nameof(MediaPlayer.PositionChanged));
            var timeChanged = VLCEvent(nameof(MediaPlayer.TimeChanged));
            var lengthChanged = VLCEvent(nameof(MediaPlayer.LengthChanged));
            var playingChanged = VLCEvent(nameof(MediaPlayer.Playing));
            var pausedChanged = VLCEvent(nameof(MediaPlayer.Paused));
            var endReachedChanged = VLCEvent(nameof(MediaPlayer.EndReached));
            var stoppedChanged = VLCEvent(nameof(MediaPlayer.Stopped));
            var volumeChanged = Observable.Merge(VLCEvent(nameof(MediaPlayer.VolumeChanged)), playingChanged);
            var stateChanged = Observable.Merge(playingChanged, stoppedChanged, endReachedChanged, pausedChanged);
            var audioTrackChanged = this.WhenAnyValue(v => v.SelectedAudioTrack).Select(_ => Unit.Default);
            var subtitleTrackChanged = this.WhenAnyValue(v => v.SelectedSubtitleTrack).Select(_ => Unit.Default);
            var encounteredError = VLCEvent(nameof(MediaPlayer.EncounteredError));

            _subscriptions = new CompositeDisposable
            {
                Wrap(positionChanged)       .DistinctUntilChanged(_=>Position)              .Subscribe(_=>this.RaisePropertyChanged(nameof(Position))),
                Wrap(timeChanged)           .DistinctUntilChanged(_=>CurrentTime)           .Subscribe(_=>this.RaisePropertyChanged(nameof(CurrentTime))),
                Wrap(lengthChanged)         .DistinctUntilChanged(_=>Duration)              .Subscribe(_=>this.RaisePropertyChanged(nameof(Duration))),
                Wrap(volumeChanged)         .DistinctUntilChanged(_=>Volume)                .Subscribe(_=>{ if(Volume >= 0)this.RaisePropertyChanged(nameof(Volume)); }),
                Wrap(playingChanged)        .DistinctUntilChanged(_=>AudioTrack)            .Subscribe(_=>this.RaisePropertyChanged(nameof(AudioTrack))),
                Wrap(playingChanged)        .DistinctUntilChanged(_=>SubtitleTrack)         .Subscribe(_=>this.RaisePropertyChanged(nameof(SubtitleTrack))),
                Wrap(audioTrackChanged)     .DistinctUntilChanged(_=>SelectedAudioTrack)    .Subscribe(_=>{if(SelectedAudioTrack.HasValue){MediaPlayer.SetAudioTrack(SelectedAudioTrack.Value.Id); }}),
                Wrap(subtitleTrackChanged)  .DistinctUntilChanged(_=>SelectedSubtitleTrack) .Subscribe(_=>{if(SelectedSubtitleTrack.HasValue){MediaPlayer.SetSpu(SelectedSubtitleTrack.Value.Id); }}),
                Wrap(stateChanged)          .DistinctUntilChanged(_=>IsPlaying)             .Subscribe(_=>this.RaisePropertyChanged(nameof(IsPlaying))),
                Wrap(encounteredError)      .DistinctUntilChanged(_=>IsPlaying)             .Subscribe(_=>Debug.WriteLine("Media Player Encountered Error")),
                endReachedChanged           .Subscribe(_ =>{ InteralPosition = 1.0f; this.RaisePropertyChanged(nameof(Position)); })
            };

            bool active() => _subscriptions == null ? false : MediaPlayer.IsPlaying || MediaPlayer.CanPause;
            stateChanged = Wrap(stateChanged);

            PlayOrPauseVideoCommand = ReactiveCommand.Create(() => Operating(() =>
            {
                switch (MediaPlayer.State)
                {
                    case VLCState.Playing:
                    case VLCState.Paused:
                        MediaPlayer.Pause();
                        break;
                    case VLCState.NothingSpecial:
                    case VLCState.Stopped:
                    case VLCState.Ended:
                        MediaPlayer.Play(MediaPlayer.Media!);
                        break;
                }
            }));

            ForwardCommand = ReactiveCommand.Create(
                () => MediaPlayer.Time += _config.moveStep * 1000,
                stateChanged.Select(_ => active()));

            BackwardCommand = ReactiveCommand.Create(
                () => MediaPlayer.Time -= _config.moveStep * 1000,
                stateChanged.Select(_ => active()));

            NextFrameCommand = ReactiveCommand.Create(
                () => MediaPlayer.NextFrame(),
                stateChanged.Select(_ => active()));
            #endregion

            #region Init Data Grid
            SelectedOutputFiles = new ObservableCollection<OutputFile>();
            var selectedProjectChanged = Observable.FromEventPattern(SelectedProject, nameof(SelectedProject.CollectionChanged)).Select(_ => SelectedProject.Count > 0);
            var selectedOutputFilesChanged = Observable.FromEventPattern(SelectedOutputFiles, nameof(SelectedOutputFiles.CollectionChanged)).Select(_ => SelectedOutputFiles.Count > 0);

            AddOutputFilesCommand = ReactiveCommand.Create(
                () => 
                    { 
                        SelectedProject.First().AddChild();
                        SelectedSingleOutputFile = SelectedProject.First().GetLastChild();
                    },
                selectedProjectChanged);

            ExportCommand = ReactiveCommand.Create(
                async () =>
                {
                    string folderFullName = await FileHandler.SelectExportFolder();
                    ExportHandler.GenerateExportInfo(folderFullName, SelectedOutputFiles.ToList());
                    IsExporting = true;
                    await ExportHandler.ExecuteFFmpeg();
                    IsExporting = false;
                }, selectedOutputFilesChanged);

            CencelCommand = ReactiveCommand.Create(
                () => { ExportHandler.CencelExport(); IsExporting = false; });
            #endregion
        }

        public void AutoComplateInTime()
        {
            if (SelectedOutputFiles.Count <= 0)
                return;
            foreach (var file in SelectedOutputFiles)
            {
                file.Edit_InTime = CurrentTime;
            }
        }

        public void AutoComplateOutTime()
        {
            if (SelectedOutputFiles.Count <= 0)
                return;
            foreach (var file in SelectedOutputFiles)
            {
                file.Edit_OutTime = CurrentTime;
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _subscriptions = null;
            MediaPlayer.Dispose();
        }
    }
}
