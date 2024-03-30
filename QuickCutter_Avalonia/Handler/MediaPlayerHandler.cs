using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Mode;
using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using QuickCutter_Avalonia.ViewModels;

namespace QuickCutter_Avalonia.Handler
{
    internal static class MediaPlayerHandler
    {
        static private Config m_Config;
        static private LibVLC m_libVLC;
        static public VlcMediaplayerViewModel HostedVlcMediaplayerViewModel { get; private set; }

        static private Subject<Unit> m_StateChangedRefresh;
        static private CompositeDisposable m_Subscriptions;

        static private Action? m_RelpayStopAction;
        static private bool m_CanUpdateUI = true;

        static private IObservable<Unit> Wrap(IObservable<Unit> source)
        {
            return source.Merge(m_StateChangedRefresh) // something need to refresh when state changed.
                .ObserveOn(AvaloniaScheduler.Instance); // make sure call on UI Thread
        }

        static private IObservable<Unit> VLCEvent(string name)
        {
            return Observable.FromEventPattern(HostedVlcMediaplayerViewModel.Player, name).Select(_ => Unit.Default);
        }

        static public void InitMediaPlayer(object? obj)
        {
            if (obj == null)
            {
                return;
            }

            HostedVlcMediaplayerViewModel = obj as VlcMediaplayerViewModel;
            m_Config = Utils.GetConfig();
            m_libVLC = new LibVLC("--freetype-rel-fontsize=25");
            HostedVlcMediaplayerViewModel.Player = new MediaPlayer(m_libVLC);
            //HostedVLCMediaplayer = hostedVLCMediaplayer;

            m_StateChangedRefresh = new Subject<Unit>();

            var positioChanged = Wrap(VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.PositionChanged)));
            var volumeChanged = Wrap(VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.VolumeChanged)));
            var timeChanged = Wrap(VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.TimeChanged)));
            var durationChanged = Wrap(VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.LengthChanged)));

            var startedPlayback = VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.Playing));
            var pausedPlayback = VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.Paused));
            var stoppedPlayback = VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.Stopped));
            var endReached = VLCEvent(nameof(HostedVlcMediaplayerViewModel.Player.EndReached));
            var stateChanged = Observable.Merge(startedPlayback, pausedPlayback, endReached, stoppedPlayback)
                .Where(_ => m_CanUpdateUI);

            m_Subscriptions = new CompositeDisposable
            {
                positioChanged.Subscribe(_ =>
                {
                    HostedVlcMediaplayerViewModel.UpdateUiPosition();
                    Debug.WriteLine($"Run on tread {Environment.CurrentManagedThreadId}");
                }),
                volumeChanged.Subscribe(_ => HostedVlcMediaplayerViewModel.UpdateUiVolume()),
                timeChanged.Subscribe(_ =>
                {
                    HostedVlcMediaplayerViewModel.UpdateCurrentTime();
                    m_RelpayStopAction?.Invoke();
                }),
                durationChanged.Subscribe(_ => HostedVlcMediaplayerViewModel.UpdateUiDuration()),
                endReached.Subscribe(_ => ThreadPool.QueueUserWorkItem(_ => ReloadMedia())),
                stateChanged.Subscribe(_ =>
                {
                    Debug.WriteLine("========Update UI========");
                    Debug.WriteLine($"Current State: {HostedVlcMediaplayerViewModel.Player.State}");
                    Debug.WriteLine($"Run on tread {Environment.CurrentManagedThreadId}");
                    HostedVlcMediaplayerViewModel.UpdateUiPlayingState();
                    m_StateChangedRefresh.OnNext(Unit.Default);
                })
            };
        }

        // Some libvlc methods will not have any effect when the video is paused, such as SeekTo()
        // Update event(such as PositionChanged, TimeChanged .etc) will not be call when change Time in MediaPlayer when the video is paused.
        // This methods let you to do this operation when the video is playing.
        private static async void ReadyForPlay(Action action, bool pauseAfterAction = true)
        {
            m_CanUpdateUI = false;
            HostedVlcMediaplayerViewModel.Player.Play();
            while (!HostedVlcMediaplayerViewModel.Player.IsPlaying && HostedVlcMediaplayerViewModel.Player.Media != null)
            {
                Debug.WriteLine("Wait for begin play...");
                await Task.Delay(25);
            }

            if (HostedVlcMediaplayerViewModel.Player.Media != null)
            {
                action();
                if (pauseAfterAction)
                    HostedVlcMediaplayerViewModel.Player.Pause();
            }

            m_CanUpdateUI = true;
        }

        public static void DisposeMediaPlayerHandler()
        {
            m_Subscriptions.Dispose();
            m_libVLC?.Dispose();
        }

        public static void LoadMedia(Uri filePath)
        {
            HostedVlcMediaplayerViewModel.Player.Media = new Media(m_libVLC, filePath);
            ReadyForPlay(() =>
            {
                HostedVlcMediaplayerViewModel.UpdateUiAudioTrackOptions();
                HostedVlcMediaplayerViewModel.UpdateUiSubtitleTrackOptions();
                HostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.Zero);
            }, !m_Config.autoPlay);
        }

        static public void ReloadMedia()
        {
            Debug.WriteLine($"ReloadMedia() runs on Thread {Environment.CurrentManagedThreadId}");

            HostedVlcMediaplayerViewModel.Player.Stop();

            Debug.WriteLine($"Media stop and return on Thread {Environment.CurrentManagedThreadId}");
            ReadyForPlay(() =>
            {
                HostedVlcMediaplayerViewModel.UpdateUiAudioTrackOptions();
                HostedVlcMediaplayerViewModel.UpdateUiSubtitleTrackOptions();
                HostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.Zero);
            }, !m_Config.loopPlayback);
        }

        static public void ResetMediaPlayer()
        {
            HostedVlcMediaplayerViewModel.Player.Stop();
            HostedVlcMediaplayerViewModel.UpdateUiAudioTrackOptions();
            HostedVlcMediaplayerViewModel.UpdateUiSubtitleTrackOptions();
            HostedVlcMediaplayerViewModel.Player.Media = null;
        }

        static public void TogglePlay()
        {
            switch (HostedVlcMediaplayerViewModel.Player.State)
            {
                case VLCState.Playing:
                case VLCState.Paused:
                    HostedVlcMediaplayerViewModel.Player.Pause();
                    break;
                case VLCState.NothingSpecial:
                case VLCState.Stopped:
                case VLCState.Ended:
                    HostedVlcMediaplayerViewModel.Player.Play();
                    break;
            }
        }

        static public void MoveForward(int deltaTime)
        {
            var t = HostedVlcMediaplayerViewModel.Player.Length - HostedVlcMediaplayerViewModel.Player.Time;
            if (HostedVlcMediaplayerViewModel.Player.State == VLCState.Playing)
            {
                HostedVlcMediaplayerViewModel.Player.Time = t <= deltaTime
                    ? HostedVlcMediaplayerViewModel.Player.Length
                    : HostedVlcMediaplayerViewModel.Player.Time + deltaTime;
            }
            else if
                (HostedVlcMediaplayerViewModel.Player.State ==
                 VLCState.Paused) // UI will not update when paused, so we need to play first and then change time.
            {
                ReadyForPlay(() => HostedVlcMediaplayerViewModel.Player.Time = t <= deltaTime
                    ? HostedVlcMediaplayerViewModel.Player.Length
                    : HostedVlcMediaplayerViewModel.Player.Time + deltaTime);
            }
        }

        static public void MoveBackward(int deltaTime)
        {
            if (HostedVlcMediaplayerViewModel.Player.State == VLCState.Playing)
            {
                HostedVlcMediaplayerViewModel.Player.Time = HostedVlcMediaplayerViewModel.Player.Time <= deltaTime
                    ? 0
                    : HostedVlcMediaplayerViewModel.Player.Time - deltaTime;
            }
            else if
                (HostedVlcMediaplayerViewModel.Player.State ==
                 VLCState.Paused) // UI will not update when paused, so we need to play first and then change time.
            {
                ReadyForPlay(() => HostedVlcMediaplayerViewModel.Player.Time = HostedVlcMediaplayerViewModel.Player.Time <= deltaTime
                    ? 0
                    : HostedVlcMediaplayerViewModel.Player.Time - deltaTime);
            }
        }

        static public void Replay(long startTime, long endTime)
        {
            if (HostedVlcMediaplayerViewModel.Player.State == VLCState.Playing)
            {
                HostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.FromMilliseconds(startTime));
            }
            else if (HostedVlcMediaplayerViewModel.Player.State == VLCState.Paused)
            {
                HostedVlcMediaplayerViewModel.Player.Pause();
                HostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.FromMilliseconds(startTime));
            }

            m_RelpayStopAction = () =>
            {
                if (HostedVlcMediaplayerViewModel.Player.Time < startTime - 500 ||
                    HostedVlcMediaplayerViewModel.Player.Time > endTime + 500)
                {
                    m_RelpayStopAction = null;
                    return;
                }

                if (HostedVlcMediaplayerViewModel.Player.Time >= endTime)
                {
                    HostedVlcMediaplayerViewModel.Player.Pause();
                    m_RelpayStopAction = null;
                }
            };
        }
    }
}