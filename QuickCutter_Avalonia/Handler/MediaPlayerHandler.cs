using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
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
        private static LibVLC? _libVlc;
        private static VlcMediaplayerViewModel? _hostedVlcMediaplayerViewModel;
        private static Subject<Unit>? _stateChangedRefresh;
        private static CompositeDisposable? _subscriptions;
        private static Action? _relpayStopAction;
        private static bool _canUpdateUi = true;

        private static IObservable<Unit> Wrap(IObservable<Unit> source)
        {
            return source.Merge(_stateChangedRefresh) // something need to refresh when state changed.
                .ObserveOn(AvaloniaScheduler.Instance); // make sure call on UI Thread
        }

        private static IObservable<Unit> VlcEvent(string name)
        {
            return Observable.FromEventPattern(_hostedVlcMediaplayerViewModel.Player, name).Select(_ => Unit.Default);
        }

        private static IObservable<VLCState> VlcStateEvent(string name)
            => Observable.FromEventPattern(_hostedVlcMediaplayerViewModel.Player, name)
                .Select(_ => _hostedVlcMediaplayerViewModel.Player.State);
        public static void InitMediaPlayer(VlcMediaplayerViewModel vlcMediaplayerViewModel)
        {
            Console.WriteLine($"Media player init on tread {Environment.CurrentManagedThreadId}");
            _hostedVlcMediaplayerViewModel = vlcMediaplayerViewModel;
            _libVlc = new LibVLC("--freetype-rel-fontsize=25");
            _hostedVlcMediaplayerViewModel.Player = new MediaPlayer(_libVlc);
            //HostedVLCMediaplayer = hostedVLCMediaplayer;

            _stateChangedRefresh = new Subject<Unit>();

            var positioChanged = Wrap(VlcEvent(nameof(_hostedVlcMediaplayerViewModel.Player.PositionChanged)));
            var volumeChanged = Wrap(VlcEvent(nameof(_hostedVlcMediaplayerViewModel.Player.VolumeChanged)));
            var timeChanged = Wrap(VlcEvent(nameof(_hostedVlcMediaplayerViewModel.Player.TimeChanged)));
            var durationChanged = Wrap(VlcEvent(nameof(_hostedVlcMediaplayerViewModel.Player.LengthChanged)));

            var startedPlayback = VlcStateEvent(nameof(_hostedVlcMediaplayerViewModel.Player.Playing));
            var pausedPlayback = VlcStateEvent(nameof(_hostedVlcMediaplayerViewModel.Player.Paused));
            var stoppedPlayback = VlcStateEvent(nameof(_hostedVlcMediaplayerViewModel.Player.Stopped));
            var endReached = VlcStateEvent(nameof(_hostedVlcMediaplayerViewModel.Player.EndReached));
            var stateChanged = Observable.Merge(startedPlayback, pausedPlayback, endReached, stoppedPlayback)
                .Where(_ => _canUpdateUi);

            _subscriptions = new CompositeDisposable
            {
                positioChanged.Subscribe(_ => _hostedVlcMediaplayerViewModel.UpdateUiPosition()),
                volumeChanged.Subscribe(_ => _hostedVlcMediaplayerViewModel.UpdateUiVolume()),
                timeChanged.Subscribe(_ =>
                {
                    _hostedVlcMediaplayerViewModel.UpdateCurrentTime();
                    _relpayStopAction?.Invoke();
                }),
                durationChanged.Subscribe(_ => _hostedVlcMediaplayerViewModel.UpdateUiDuration()),
                stateChanged.Subscribe(state =>
                {
                    Console.WriteLine($"Current State [{state}] @ tread {Environment.CurrentManagedThreadId}");
                    switch (state)
                    {
                        case VLCState.Playing:
                        case VLCState.Paused:
                            _hostedVlcMediaplayerViewModel.UpdateUiPlayingState();
                            _hostedVlcMediaplayerViewModel.UpdateUiVolume();
                            break;
                        case VLCState.Ended:
                            ThreadPool.QueueUserWorkItem(_ => ReloadMedia());
                            break;
                    }
                })
            };
        }

        // Some libvlc methods will not have any effect when the video is paused, such as SeekTo()
        // Update event(such as PositionChanged, TimeChanged .etc) will not be call when change Time in MediaPlayer when the video is paused.
        // This methods let you to do this operation when the video is playing.
        private static async void ReadyForPlay(Action action, bool pauseAfterAction = true)
        {
            _canUpdateUi = false;
            _hostedVlcMediaplayerViewModel.Player.Play();
            while (!_hostedVlcMediaplayerViewModel.Player.IsPlaying && _hostedVlcMediaplayerViewModel.Player.Media != null)
            {
                Console.WriteLine($"Wait for begin play on thread {Environment.CurrentManagedThreadId}");
                await Task.Delay(25);
            }

            if (_hostedVlcMediaplayerViewModel.Player.Media != null)
            {
                action();
                if (pauseAfterAction)
                    _hostedVlcMediaplayerViewModel.Player.Pause();
            }

            _canUpdateUi = true;
        }

        public static void DisposeMediaPlayerHandler()
        {
            _subscriptions?.Dispose();
            _libVlc?.Dispose();
        }

        public static void LoadMedia(Uri filePath)
        {
            _hostedVlcMediaplayerViewModel.Player.Media = new Media(_libVlc, filePath);
            ReadyForPlay(() =>
            {
                _hostedVlcMediaplayerViewModel.UpdateUiAudioTrackOptions();
                _hostedVlcMediaplayerViewModel.UpdateUiSubtitleTrackOptions();
                _hostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.Zero);
            }, !Utils.GetConfig().autoPlay);
        }

        private static void ReloadMedia()
        {
            Console.WriteLine($"ReloadMedia() Called on Thread {Environment.CurrentManagedThreadId}");

            _hostedVlcMediaplayerViewModel.Player.Stop();
            ReadyForPlay(() =>
            {
                _hostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.Zero);
            }, !Utils.GetConfig().loopPlayback);
        }

        public static void ResetMediaPlayer()
        {
            _hostedVlcMediaplayerViewModel.Player.Stop();
            _hostedVlcMediaplayerViewModel.UpdateUiAudioTrackOptions();
            _hostedVlcMediaplayerViewModel.UpdateUiSubtitleTrackOptions();
            _hostedVlcMediaplayerViewModel.Player.Media = null;
        }

        public static void TogglePlay()
        {
            Console.WriteLine($"TogglePlayOrPause Called on Tread {Environment.CurrentManagedThreadId}");
            switch (_hostedVlcMediaplayerViewModel.Player.State)
            {
                case VLCState.Playing:
                case VLCState.Paused:
                    _hostedVlcMediaplayerViewModel.Player.Pause();
                    break;
                case VLCState.NothingSpecial:
                case VLCState.Stopped:
                case VLCState.Ended:
                    _hostedVlcMediaplayerViewModel.Player.Play();
                    break;
            }
        }

        public static void MoveForward(int deltaTime)
        {
            var t = _hostedVlcMediaplayerViewModel.Player.Length - _hostedVlcMediaplayerViewModel.Player.Time;
            if (_hostedVlcMediaplayerViewModel.Player.State == VLCState.Playing)
            {
                _hostedVlcMediaplayerViewModel.Player.Time = t <= deltaTime
                    ? _hostedVlcMediaplayerViewModel.Player.Length
                    : _hostedVlcMediaplayerViewModel.Player.Time + deltaTime;
            }
            else if
                (_hostedVlcMediaplayerViewModel.Player.State ==
                 VLCState.Paused) // UI will not update when paused, so we need to play first and then change time.
            {
                ReadyForPlay(() => _hostedVlcMediaplayerViewModel.Player.Time = t <= deltaTime
                    ? _hostedVlcMediaplayerViewModel.Player.Length
                    : _hostedVlcMediaplayerViewModel.Player.Time + deltaTime);
            }
        }

        public static void MoveBackward(int deltaTime)
        {
            if (_hostedVlcMediaplayerViewModel.Player.State == VLCState.Playing)
            {
                _hostedVlcMediaplayerViewModel.Player.Time = _hostedVlcMediaplayerViewModel.Player.Time <= deltaTime
                    ? 0
                    : _hostedVlcMediaplayerViewModel.Player.Time - deltaTime;
            }
            else if
                (_hostedVlcMediaplayerViewModel.Player.State ==
                 VLCState.Paused) // UI will not update when paused, so we need to play first and then change time.
            {
                ReadyForPlay(() => _hostedVlcMediaplayerViewModel.Player.Time = _hostedVlcMediaplayerViewModel.Player.Time <= deltaTime
                    ? 0
                    : _hostedVlcMediaplayerViewModel.Player.Time - deltaTime);
            }
        }

        public static void Replay(long startTime, long endTime)
        {
            if (_hostedVlcMediaplayerViewModel.Player.State == VLCState.Playing)
            {
                _hostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.FromMilliseconds(startTime));
            }
            else if (_hostedVlcMediaplayerViewModel.Player.State == VLCState.Paused)
            {
                _hostedVlcMediaplayerViewModel.Player.Pause();
                _hostedVlcMediaplayerViewModel.Player.SeekTo(TimeSpan.FromMilliseconds(startTime));
            }

            _relpayStopAction = () =>
            {
                if (_hostedVlcMediaplayerViewModel.Player.Time < startTime - 500 ||
                    _hostedVlcMediaplayerViewModel.Player.Time > endTime + 500)
                {
                    _relpayStopAction = null;
                    return;
                }

                if (_hostedVlcMediaplayerViewModel.Player.Time >= endTime)
                {
                    _hostedVlcMediaplayerViewModel.Player.Pause();
                    _relpayStopAction = null;
                }
            };
        }
    }
}