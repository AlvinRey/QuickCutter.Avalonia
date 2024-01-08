﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using QuickCutter_Avalonia.Models;
using System.Diagnostics;
using LibVLCSharp.Shared;
using System;
using LibVLCSharp.Shared.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using System.Reactive.Disposables;





namespace QuickCutter_Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject, IDisposable
    {
        #region Project List
        [Reactive]
        public Project? SelectedProject { get; set; }
        public ObservableCollection<Project> Projects { get; }

        public void ImportProjectFile(VideoInfo videoInfo)
        {
            Projects.Add(new Project(videoInfo));
        }
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
            var media = new Media(_libVlc, new Uri(SelectedProject!.ImportVideoInfo.VideoFullName!));
            MediaPlayer.Media = media;

            //MediaPlayer.Play();
        }
        public void ResetMediaPlayer()
        {
            MediaPlayer.Stop();
            MediaPlayer.Media = null;
            refresh.OnNext(Unit.Default);
        }
        #endregion

        #region Output Data Grid
        [Reactive]
        public OutputFile? SelectedOutputFile { get; set; }

        #endregion

        public MainWindowViewModel()
        {
            Projects = new ObservableCollection<Project>();

            #region Init Media Player
            _libVlc = new LibVLC();
            MediaPlayer = new MediaPlayer(_libVlc);
            refresh = new Subject<Unit>();
            bool operationActive = false;


            IObservable<Unit> Wrap(IObservable<Unit> source)
                => source.Where(_ => !operationActive).Merge(refresh).ObserveOn(AvaloniaScheduler.Instance);

            IObservable<Unit> VLCEvent(string name)
                => Observable.FromEventPattern(MediaPlayer, name).Select(_ => Unit.Default);

            void Op(Action action)
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
            var volumeChanged = Observable.Merge(VLCEvent(nameof(MediaPlayer.VolumeChanged)),playingChanged);
            var stateChanged = Observable.Merge(playingChanged, stoppedChanged, endReachedChanged, pausedChanged);
            var audioTrackChanged = this.WhenAnyValue(v => v.SelectedAudioTrack).Select(_ => Unit.Default);
            var subtitleTrackChanged = this.WhenAnyValue(v => v.SelectedSubtitleTrack).Select(_ => Unit.Default);

            _subscriptions = new CompositeDisposable
            {
                Wrap(positionChanged)       .DistinctUntilChanged(_=>Position)              .Subscribe(_=>this.RaisePropertyChanged(nameof(Position))),
                Wrap(timeChanged)           .DistinctUntilChanged(_=>CurrentTime)           .Subscribe(_=>this.RaisePropertyChanged(nameof(CurrentTime))),
                Wrap(lengthChanged)         .DistinctUntilChanged(_=>Duration)              .Subscribe(_=>this.RaisePropertyChanged(nameof(Duration))),
                Wrap(volumeChanged)         .DistinctUntilChanged(_=>Volume)                .Subscribe(_=>this.RaisePropertyChanged(nameof(Volume))),
                Wrap(playingChanged)        .DistinctUntilChanged(_=>AudioTrack)            .Subscribe(_=>this.RaisePropertyChanged(nameof(AudioTrack))),
                Wrap(playingChanged)        .DistinctUntilChanged(_=>SubtitleTrack)         .Subscribe(_=>this.RaisePropertyChanged(nameof(SubtitleTrack))),
                Wrap(audioTrackChanged)     .DistinctUntilChanged(_=>SelectedAudioTrack)    .Subscribe(_=>{if(SelectedAudioTrack.HasValue){MediaPlayer.SetAudioTrack(SelectedAudioTrack.Value.Id); }}),
                Wrap(subtitleTrackChanged)  .DistinctUntilChanged(_=>SelectedSubtitleTrack) .Subscribe(_=>{if(SelectedSubtitleTrack.HasValue){MediaPlayer.SetSpu(SelectedSubtitleTrack.Value.Id); }}),
                Wrap(stateChanged)          .DistinctUntilChanged(_=>IsPlaying)             .Subscribe(_=>this.RaisePropertyChanged(nameof(IsPlaying)))
            };

            bool active() => _subscriptions == null ? false : MediaPlayer.IsPlaying || MediaPlayer.CanPause;
            stateChanged = Wrap(stateChanged);

            PlayOrPauseVideoCommand = ReactiveCommand.Create(() => Op(() =>
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
                () => MediaPlayer.Time += 1000,
                stateChanged.Select(_ => active()));

            BackwardCommand = ReactiveCommand.Create(
                () => MediaPlayer.Time -= 1000,
                stateChanged.Select(_ => active()));

            NextFrameCommand = ReactiveCommand.Create(
                () => MediaPlayer.NextFrame(),
                stateChanged.Select(_ => active()));
            #endregion


        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _subscriptions = null;
            MediaPlayer.Dispose();
        }
    }
}
