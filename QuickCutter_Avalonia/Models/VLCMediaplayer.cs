using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using QuickCutter_Avalonia.Handler;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Xml.Linq;

namespace QuickCutter_Avalonia.Models
{
    public class VLCMediaplayer : ReactiveObject
    {
        public MediaPlayer Player { get; set; }
        private float m_Position;
        public float Position 
        { 
            get => m_Position;
            set 
            { 
                this.RaiseAndSetIfChanged(ref m_Position, value, nameof(Position));
                if(m_Position != Player.Position)
                    Player.Position = value;
            }
        }
        private int m_Volume;
        public int Volume
        {
            get => m_Volume;
            set
            {
                this.RaiseAndSetIfChanged(ref m_Volume, value, nameof(Volume));
                if (m_Volume != Player.Volume)
                    Player.Volume = value;
            }
        }
        public IEnumerable<TrackDescription> AudioTrack
        {
            get
            {
                return Player.AudioTrackDescription.AsEnumerable();
            }
        }
        private TrackDescription? m_SelectedAudioTrack;
        public TrackDescription? SelectedAudioTrack 
        {
            get => m_SelectedAudioTrack;
            set
            {
                this.RaiseAndSetIfChanged(ref m_SelectedAudioTrack, value, nameof(SelectedAudioTrack));
                if (m_SelectedAudioTrack.HasValue && m_SelectedAudioTrack.Value.Id != Player.AudioTrack)
                {
                    Player.SetAudioTrack(m_SelectedAudioTrack.Value.Id);
                }
            }
        }

        public IEnumerable<TrackDescription> SubtitleTrack
        {
            get
            {
                return Player.SpuDescription.AsEnumerable();
            }
        }
        public TrackDescription? m_SelectedSubtitleTrack;
        public TrackDescription? SelectedSubtitleTrack
        {
            get => m_SelectedSubtitleTrack;
            set
            {
                this.RaiseAndSetIfChanged(ref m_SelectedSubtitleTrack, value, nameof(SelectedSubtitleTrack));
                if (m_SelectedSubtitleTrack.HasValue && m_SelectedSubtitleTrack.Value.Id != Player.Spu)
                {
                    Player.SetSpu(m_SelectedSubtitleTrack.Value.Id);
                }
            }
        }

        [Reactive]
        public TimeSpan CurrentTime { get; set; }
        [Reactive]
        public TimeSpan Duration { get; set; }
        [Reactive]
        public bool IsPlaying { get; set; }


        public VLCMediaplayer()
        {
            //ThreadPool.QueueUserWorkItem(_ => MediaPlayerHandler.InitMediaPlayer(this));
            //var t = new TimeCounter("InitMediaPlayer");
            DebugHandler.StopwatchStart();
            MediaPlayerHandler.InitMediaPlayer(this);
            DebugHandler.StopwatchStopAndPrintTime();
        }

        ~VLCMediaplayer()
        {
            MediaPlayerHandler.DisposeMediaPlayerHandler();
        }

        public void UpdateUIPosition()
        {
            Position = Player.Position;
        }

        public void UpdateUIVolume()
        {
            if (Player.Volume >= 0) Volume = Player.Volume;
        }

        public void UpdateCurrentTime()
        {
            CurrentTime = TimeSpan.FromMilliseconds(Player.Time > -1 ? Player.Time : 0);
        }

        public void UpdateUIDuration()
        {
            Duration = TimeSpan.FromMilliseconds(Player.Length > -1 ? Player.Length : 0);
        }

        public void UpdateUIPlayingState()
        {
            Debug.WriteLine("[Update UI] Play State");
            IsPlaying = Player.IsPlaying;
        }

        public void UpdateUIAudioTrackOptions()
        {
            Debug.WriteLine("[Update UI] Audio Track Options");
            this.RaisePropertyChanged(nameof(AudioTrack));
            foreach (var track in Player.AudioTrackDescription)
            {
                if (track.Id == Player.AudioTrack)
                {
                    SelectedAudioTrack = track;
                    break;
                }
            }
        }

        public void UpdateUISubtitleTrackOptions()
        {
            Debug.WriteLine("[Update UI] Subtitle Track Options");
            this.RaisePropertyChanged(nameof(SubtitleTrack));
            foreach (var track in Player.SpuDescription)
            {
                if (track.Id == Player.Spu)
                {
                    SelectedSubtitleTrack = track;
                    break;
                }
            }
        }
    }
}
