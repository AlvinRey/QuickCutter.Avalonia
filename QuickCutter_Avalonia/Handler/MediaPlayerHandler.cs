using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Mode;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace QuickCutter_Avalonia.Handler
{
    enum MediaPlayerState
    {
        Playing = 1,
        Paused = 2,
        Stopped = 3,
        EndReached = 4
    }

    internal class MediaPlayerHandler
    {
        static private Config m_Config;
        static private LibVLC m_libVLC;
        static public VLCMediaplayer HostedVLCMediaplayer { get; private set; }

        static private MediaPlayerState m_MediaPlayerState = MediaPlayerState.Stopped;
        static private Subject<Unit> m_StateChangedRefresh;
        static private CompositeDisposable m_Subscriptions;
        static public IObservable<Unit> StateChangedObservable { get; private set; }


        static public void InitMediaPlayer(VLCMediaplayer hostedVLCMediaplayer)
        {
            m_Config = Utils.GetConfig();
            string adcanceOptions = string.Empty;
            if (false)
            {
                adcanceOptions += "--input-repeat=65535";
            }
            m_libVLC = new LibVLC(adcanceOptions);
            hostedVLCMediaplayer.Player = new MediaPlayer(m_libVLC);
            HostedVLCMediaplayer = hostedVLCMediaplayer;

            m_StateChangedRefresh = new Subject<Unit>();
            IObservable<Unit> Wrap(IObservable<Unit> source) // something need to refresh when state changed.
                => source.Merge(m_StateChangedRefresh).ObserveOn(AvaloniaScheduler.Instance);

            IObservable<Unit> VLCEvent(string name)
                => Observable.FromEventPattern(HostedVLCMediaplayer.Player, name).Select(_ => Unit.Default);

            var positioChanged = Wrap(VLCEvent(nameof(HostedVLCMediaplayer.Player.PositionChanged)));
            var volumeChanged = Wrap(VLCEvent(nameof(HostedVLCMediaplayer.Player.VolumeChanged)));
            var timeChanged = Wrap(VLCEvent(nameof(HostedVLCMediaplayer.Player.TimeChanged)));
            var durationChanged = Wrap(VLCEvent(nameof(HostedVLCMediaplayer.Player.LengthChanged)));


            var startedPlayback = VLCEvent(nameof(HostedVLCMediaplayer.Player.Playing));
            var pausedPlayback = VLCEvent(nameof(HostedVLCMediaplayer.Player.Paused));
            var stoppedPlayback = VLCEvent(nameof(HostedVLCMediaplayer.Player.Stopped));
            var endReached = VLCEvent(nameof(HostedVLCMediaplayer.Player.EndReached));
            StateChangedObservable = Observable.Merge(startedPlayback, pausedPlayback, stoppedPlayback, endReached);

            m_Subscriptions = new CompositeDisposable
            {
                positioChanged.Subscribe(_=> HostedVLCMediaplayer.UpdateUIPosition()),
                volumeChanged.Subscribe(_=> HostedVLCMediaplayer.UpdateUIVolume()),
                timeChanged.Subscribe(_=> HostedVLCMediaplayer.UpdateCurrentTime()),
                durationChanged.Subscribe(_=>HostedVLCMediaplayer.UpdateUIDuration()),

                StateChangedObservable.Subscribe(_=>
                {
                    HostedVLCMediaplayer.UpdateUIPlayingState();
                    HostedVLCMediaplayer.UpdateUIAudioTrackOptions();
                    HostedVLCMediaplayer.UpdateUISubtitleTrackOptions();
                    m_StateChangedRefresh.OnNext(Unit.Default);
                })
            };

        }

        static public void DisposeMediaPlayerHandler()
        {
            m_Subscriptions.Dispose();
            m_libVLC?.Dispose();
        }

        static private void SetMediaPlayerState(MediaPlayerState state)
        {
            m_MediaPlayerState = state;
            switch (m_MediaPlayerState)
            {
                case MediaPlayerState.Playing:
                    HostedVLCMediaplayer.IsPlaying = true;
                    break;
                case MediaPlayerState.Paused:
                case MediaPlayerState.Stopped:
                case MediaPlayerState.EndReached:
                    HostedVLCMediaplayer.IsPlaying = false;
                    break;
            }
        }

        static public void Play()
        {
            HostedVLCMediaplayer.Player.Play();
        }

        static public void LoadMedia(Uri filePath)
        {
            HostedVLCMediaplayer.Player.Media = new Media(m_libVLC, filePath);
            if (m_Config.autoPlay)
                HostedVLCMediaplayer.Player.Play();
        }

        static public void ResetMediaPlayer()
        {
            HostedVLCMediaplayer.Player.Stop();
            HostedVLCMediaplayer.Player.Media = null;
        }

        static public void TogglePlay()
        {
            switch (HostedVLCMediaplayer.Player.State)
            {
                case VLCState.Playing:
                    HostedVLCMediaplayer.Player.Pause();
                    break;
                case VLCState.NothingSpecial:
                case VLCState.Paused:
                case VLCState.Stopped:
                case VLCState.Ended:
                    HostedVLCMediaplayer.Player.Play();
                    break;
            }
        }

        static public void MoveForward(int deltaTime)
        {
            if(HostedVLCMediaplayer.Player.IsPlaying || HostedVLCMediaplayer.Player.CanPause)
                HostedVLCMediaplayer.Player.Time += deltaTime;
        }

        static public void MoveBackward(int deltaTime)
        {
            if (HostedVLCMediaplayer.Player.IsPlaying || HostedVLCMediaplayer.Player.CanPause)
                HostedVLCMediaplayer.Player.Time -= deltaTime;
        }
    }
}
