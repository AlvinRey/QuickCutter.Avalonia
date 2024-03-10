using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Mode;
using QuickCutter_Avalonia.Models;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class MediaPlayerHandler
    {
        static private Config m_Config;
        static private LibVLC m_libVLC;
        static public VLCMediaplayer HostedVLCMediaplayer { get; private set; }

        static private Subject<Unit> m_StateChangedRefresh;
        static private CompositeDisposable m_Subscriptions;

        static private Action? m_RelpayStopAction;
        static private bool m_CanUpdateUI = true;

        static public void InitMediaPlayer(object? obj)
        {
            if (obj == null)
            {
                return;
            }
            HostedVLCMediaplayer = obj as VLCMediaplayer;
            m_Config = Utils.GetConfig();
            m_libVLC = new LibVLC("--vout=glwin32");
            HostedVLCMediaplayer.Player = new MediaPlayer(m_libVLC);
            //HostedVLCMediaplayer = hostedVLCMediaplayer;

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
            var stateChanged = Observable.Merge(startedPlayback, pausedPlayback, endReached, stoppedPlayback).Where(_ => m_CanUpdateUI);

            m_Subscriptions = new CompositeDisposable
            {
                positioChanged.Subscribe(_=> HostedVLCMediaplayer.UpdateUIPosition()),
                volumeChanged.Subscribe(_=> HostedVLCMediaplayer.UpdateUIVolume()),
                timeChanged.Subscribe(_=> {HostedVLCMediaplayer.UpdateCurrentTime(); m_RelpayStopAction?.Invoke(); }),
                durationChanged.Subscribe(_=> HostedVLCMediaplayer.UpdateUIDuration()),
                endReached.Subscribe(_=> ThreadPool.QueueUserWorkItem(_=>ReloadMedia())),
                stateChanged.Subscribe(_=>
                {
                    Debug.WriteLine("========Update UI========");
                    Debug.WriteLine($"Current State: {HostedVLCMediaplayer.Player.State.ToString()}");

                        HostedVLCMediaplayer.UpdateUIPlayingState();
                        m_StateChangedRefresh.OnNext(Unit.Default);
                })
            };

        }

        // Some libvlc methods will not have any effect when the video is paused, such as SeekTo()
        // Update event(such as PositionChanged, TimeChanged .etc) will not be call when change Time in MediaPlayer when the video is paused.
        // This methods let you to do this operation when the video is playing.
        static private async void ReadyForPlay(Action action, bool pauseAfterAction = true)
        {
            m_CanUpdateUI = false;
            HostedVLCMediaplayer.Player.Play();
            while (!HostedVLCMediaplayer.Player.IsPlaying)
            {
                Debug.WriteLine("Wait for begin play...");
                await Task.Delay(25);
            }
            action();
            if (pauseAfterAction)
                HostedVLCMediaplayer.Player.Pause();
            m_CanUpdateUI = true;
        }

        static public void DisposeMediaPlayerHandler()
        {
            m_Subscriptions.Dispose();
            m_libVLC?.Dispose();
        }

        static public async void LoadMedia(Uri filePath)
        {
            HostedVLCMediaplayer.Player.Media = new Media(m_libVLC, filePath);
            ReadyForPlay(() =>
            {
                HostedVLCMediaplayer.UpdateUIAudioTrackOptions();
                HostedVLCMediaplayer.UpdateUISubtitleTrackOptions();
                HostedVLCMediaplayer.Player.SeekTo(TimeSpan.Zero);
            }, !m_Config.autoPlay);
        }

        static public void ReloadMedia()
        {
            Debug.WriteLine("Execute ReloadMedia()");
            HostedVLCMediaplayer.Player.Stop();
            ReadyForPlay(() =>
            {
                HostedVLCMediaplayer.UpdateUIAudioTrackOptions();
                HostedVLCMediaplayer.UpdateUISubtitleTrackOptions();
                HostedVLCMediaplayer.Player.SeekTo(TimeSpan.Zero);
            }, !m_Config.loopPlayback);
        }

        static public void ResetMediaPlayer()
        {
            HostedVLCMediaplayer.Player.Stop();
            HostedVLCMediaplayer.UpdateUIAudioTrackOptions();
            HostedVLCMediaplayer.UpdateUISubtitleTrackOptions();
            HostedVLCMediaplayer.Player.Media = null;
        }

        static public void TogglePlay()
        {
            switch (HostedVLCMediaplayer.Player.State)
            {
                case VLCState.Playing:
                case VLCState.Paused:
                    HostedVLCMediaplayer.Player.Pause();
                    break;
                case VLCState.NothingSpecial:
                case VLCState.Stopped:
                case VLCState.Ended:
                    HostedVLCMediaplayer.Player.Play();
                    break;
            }
        }

        static public void MoveForward(int deltaTime)
        {
            var t = HostedVLCMediaplayer.Player.Length - HostedVLCMediaplayer.Player.Time;
            if (HostedVLCMediaplayer.Player.State == VLCState.Playing)
            {

                HostedVLCMediaplayer.Player.Time = t <= deltaTime ? HostedVLCMediaplayer.Player.Length : HostedVLCMediaplayer.Player.Time + deltaTime;
            }
            else if (HostedVLCMediaplayer.Player.State == VLCState.Paused) // UI will not update when paused, so we need to play first and then change time.
            {
                ReadyForPlay(() => HostedVLCMediaplayer.Player.Time = t <= deltaTime ? HostedVLCMediaplayer.Player.Length : HostedVLCMediaplayer.Player.Time + deltaTime);
            }
        }

        static public void MoveBackward(int deltaTime)
        {
            if (HostedVLCMediaplayer.Player.State == VLCState.Playing)
            {
                HostedVLCMediaplayer.Player.Time = HostedVLCMediaplayer.Player.Time <= deltaTime ? 0 : HostedVLCMediaplayer.Player.Time - deltaTime;
            }
            else if (HostedVLCMediaplayer.Player.State == VLCState.Paused) // UI will not update when paused, so we need to play first and then change time.
            {
                ReadyForPlay(() => HostedVLCMediaplayer.Player.Time = HostedVLCMediaplayer.Player.Time <= deltaTime ? 0 : HostedVLCMediaplayer.Player.Time - deltaTime);
            }
        }

        static public void Replay(long startTime, long endTime)
        {
            if (HostedVLCMediaplayer.Player.State == VLCState.Playing)
            {
                HostedVLCMediaplayer.Player.SeekTo(TimeSpan.FromMilliseconds(startTime));
            }
            else if (HostedVLCMediaplayer.Player.State == VLCState.Paused)
            {
                HostedVLCMediaplayer.Player.Pause();
                HostedVLCMediaplayer.Player.SeekTo(TimeSpan.FromMilliseconds(startTime));
            }

            m_RelpayStopAction = () =>
            {
                if (HostedVLCMediaplayer.Player.Time < startTime - 500 || HostedVLCMediaplayer.Player.Time > endTime + 500)
                {
                    m_RelpayStopAction = null;
                    return;
                }

                if (HostedVLCMediaplayer.Player.Time >= endTime)
                {
                    HostedVLCMediaplayer.Player.Pause();
                    m_RelpayStopAction = null;
                }
            };
        }
    }
}
