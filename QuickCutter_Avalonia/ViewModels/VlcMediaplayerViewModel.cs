using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using QuickCutter_Avalonia.Handler;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace QuickCutter_Avalonia.ViewModels
{
    public class VlcMediaplayerViewModel : ReactiveObject
    {
        public MediaPlayer Player { get; set; }
        private float _position;
        public float Position 
        { 
            get => _position;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _position, value, nameof(Position));
                if(_position != Player.Position)
                    Player.Position = value;
            }
        }
        private int _volume;
        public int Volume
        {
            get => _volume;
            set
            {
                this.RaiseAndSetIfChanged(ref _volume, value, nameof(Volume));
                if (_volume != Player.Volume)
                    Player.Volume = value;
            }
        }
        public IEnumerable<TrackDescription> AudioTrack
        {
            get => Player.AudioTrackDescription.AsEnumerable();
        }
        private TrackDescription? _selectedAudioTrack;
        public TrackDescription? SelectedAudioTrack 
        {
            get => _selectedAudioTrack;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAudioTrack, value, nameof(SelectedAudioTrack));
                if (_selectedAudioTrack.HasValue && _selectedAudioTrack.Value.Id != Player.AudioTrack)
                {
                    Player.SetAudioTrack(_selectedAudioTrack.Value.Id);
                }
            }
        }

        public IEnumerable<TrackDescription> SubtitleTrack
        {
            get => Player.SpuDescription.AsEnumerable();
        }
        private TrackDescription? _selectedSubtitleTrack;
        public TrackDescription? SelectedSubtitleTrack
        {
            get => _selectedSubtitleTrack;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSubtitleTrack, value, nameof(SelectedSubtitleTrack));
                if (_selectedSubtitleTrack.HasValue && _selectedSubtitleTrack.Value.Id != Player.Spu)
                {
                    Player.SetSpu(_selectedSubtitleTrack.Value.Id);
                }
            }
        }

        [Reactive]
        public TimeSpan CurrentTime { get; set; }
        [Reactive]
        public TimeSpan Duration { get; set; }
        [Reactive]
        public bool IsPlaying { get; set; }

        public ReactiveCommand<Unit, Unit> ForwardCommand { get; }
        public ReactiveCommand<Unit, Unit> BackwardCommand { get; }
        
        public VlcMediaplayerViewModel()
        {
            var config = Utils.GetConfig();
            DebugHandler.StopwatchStart();
            MediaPlayerHandler.InitMediaPlayer(this);
            DebugHandler.StopwatchStopAndPrintTime();
            ForwardCommand = ReactiveCommand.Create(
                () => MediaPlayerHandler.MoveForward(config.moveStep * 1000));

            BackwardCommand = ReactiveCommand.Create(
                () => MediaPlayerHandler.MoveBackward(config.moveStep * 1000));
        }

        ~VlcMediaplayerViewModel()
        {
            MediaPlayerHandler.DisposeMediaPlayerHandler();
        }

        public void UpdateUiPosition()
        {
            Position = Player.Position;
        }

        public void UpdateUiVolume()
        {
            if (Player.Volume >= 0) Volume = Player.Volume;
        }

        public void UpdateCurrentTime()
        {
            CurrentTime = TimeSpan.FromMilliseconds(Player.Time > -1 ? Player.Time : 0);
        }

        public void UpdateUiDuration()
        {
            Duration = TimeSpan.FromMilliseconds(Player.Length > -1 ? Player.Length : 0);
        }

        public void UpdateUiPlayingState()
        {
            Debug.WriteLine("[Update UI] Play State");
            IsPlaying = Player.IsPlaying;
        }

        public void UpdateUiAudioTrackOptions()
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

        public void UpdateUiSubtitleTrackOptions()
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
