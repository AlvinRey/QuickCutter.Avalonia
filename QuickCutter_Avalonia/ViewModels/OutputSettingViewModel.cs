using FFMpegCore.Enums;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace QuickCutter_Avalonia.ViewModels
{
    public class OutputSettingViewModel : ViewModelBase
    {
        private OutputSetting? mOutputSetting;

        #region Video Setting
        public IEnumerable<string> VideoCodecOptions
        {
            get => OutputSettingHandler.VideoCodecs;
        }

        public string SelectedVideoCodec
        {
            get
            {
                if (mOutputSetting is null) return null;
                return mOutputSetting.videoSetting.selectedVideoCodec;
            }

            set
            {
                if (mOutputSetting is null) return;
                this.RaiseAndSetIfChanged(ref mOutputSetting.videoSetting.selectedVideoCodec, value, nameof(SelectedVideoCodec));
                RefreshUseCodec(value);
            }
        }

        // Codec - H.264
        [Reactive]
        public bool UseH264 { get; set; } = false;

        public IEnumerable<Speed> SpeedPresetOptions
        {
            get => OutputSettingHandler.SpeedPresets;
        }

        public Speed SelectedSpeedPreset
        {
            get
            {
                if (mOutputSetting is null) return Speed.Fast;
                return mOutputSetting.videoSetting.selectedSpeedPreset;
            }

            set
            {
                if (mOutputSetting is null) return;
                this.RaiseAndSetIfChanged(ref mOutputSetting.videoSetting.selectedSpeedPreset, value, nameof(SelectedSpeedPreset));
            }
        }

        public int ConstantRateFactor
        {
            get
            {
                if (mOutputSetting is null) return 23;
                return mOutputSetting.videoSetting.constantRateFactor;
            }

            set
            {
                if (mOutputSetting is null) return;
                this.RaiseAndSetIfChanged(ref mOutputSetting.videoSetting.constantRateFactor, value, nameof(ConstantRateFactor));
            }
        }
        #endregion

        #region Audio Output

        public IReactiveCommand ClearAudioOutputCommand { get; set; }
        public bool HasAudio
        {
            get
            {
                if (mOutputSetting is null)
                    return false;
                return OutputSettingHandler.AudioStreamDictonary[mOutputSetting.key].Count > 0;
            }
        }

        public List<AudioStreamOriginalInfo> AudioOutputOptions 
        {
            get
            {
                if (mOutputSetting is null)
                    return null;
                return OutputSettingHandler.AudioStreamDictonary[mOutputSetting.key];
            } 
        }

        public ObservableCollection<AudioStreamOriginalInfo> SelectedAudioOutputs { get; set; }
        #endregion

        #region Subtitle Output

        public bool BurnSubtitle
        {
            get
            {
                if (mOutputSetting is null) return false;
                return mOutputSetting.burnSubtitle;
            }

            set
            {
                if (mOutputSetting is null) return;
                this.RaiseAndSetIfChanged(ref mOutputSetting.burnSubtitle, value, nameof(BurnSubtitle));
            }
        }

        public IReactiveCommand ClearSubtitleOutputCommand { get; set; }

        public bool HasSubtitle
        {
            get
            {
                if (mOutputSetting is null)
                    return false;
                return OutputSettingHandler.SubtitleStreamDictonary[mOutputSetting.key].Count > 0;
            }
        }

        public List<SubtitleStreamOriginalInfo> SubtitleOutputOptions
        {
            get
            {
                if (mOutputSetting is null)
                    return null;
                return OutputSettingHandler.SubtitleStreamDictonary[mOutputSetting.key];
            }
        }
        public ObservableCollection<SubtitleStreamOriginalInfo> SelectedSubtitleOutputs 
        { get; set; }
        #endregion

        public OutputSettingViewModel()
        {
            SelectedAudioOutputs = new ObservableCollection<AudioStreamOriginalInfo>();
            SelectedSubtitleOutputs = new ObservableCollection<SubtitleStreamOriginalInfo>();
            ClearAudioOutputCommand = ReactiveCommand.Create(() => SelectedAudioOutputs.Clear());
            ClearSubtitleOutputCommand = ReactiveCommand.Create(() => SelectedSubtitleOutputs.Clear());
        }



        public void LoadDisplayOutputSetting(OutputSetting outputSetting)
        {
            mOutputSetting = outputSetting;
            RefreshDisplay();
            LoadSelectedAudioOutputs();
            LoadSelectedSubtitleOutputs();
        }

        public void UnLoadDisplayOutputSetting()
        {
            SaveSelectedAudioOutputs();
            SaveSelectedSubtitleOutputs();
            mOutputSetting = null;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            this.RaisePropertyChanged(nameof(SelectedVideoCodec));
            this.RaisePropertyChanged(nameof(SelectedSpeedPreset));
            this.RaisePropertyChanged(nameof(ConstantRateFactor));
            this.RaisePropertyChanged(nameof(HasAudio));
            this.RaisePropertyChanged(nameof(AudioOutputOptions));
            this.RaisePropertyChanged(nameof(HasSubtitle));
            this.RaisePropertyChanged(nameof(BurnSubtitle));
            this.RaisePropertyChanged(nameof(SubtitleOutputOptions));
            RefreshUseCodec(SelectedVideoCodec);
        }

        private void LoadSelectedSubtitleOutputs()
        {
            if(mOutputSetting == null) return;
            SelectedSubtitleOutputs.Clear();
            foreach(var output in mOutputSetting.selectedSubtitleOutputs)
            {
                SelectedSubtitleOutputs.Add(output);
            }
        }

        private void SaveSelectedSubtitleOutputs()
        {
            if (mOutputSetting == null) return;
            mOutputSetting.selectedSubtitleOutputs.Clear();
            foreach (var output in SelectedSubtitleOutputs)
            {
                mOutputSetting.selectedSubtitleOutputs.Add(output);
            }
        }

        private void LoadSelectedAudioOutputs()
        {
            if (mOutputSetting == null) return;
            SelectedAudioOutputs.Clear();
            foreach (var output in mOutputSetting.selectedAudioOutputs)
            {
                SelectedAudioOutputs.Add(output);
            }
        }

        private void SaveSelectedAudioOutputs()
        {
            if (mOutputSetting == null) return;
            mOutputSetting.selectedAudioOutputs.Clear();
            foreach (var output in SelectedAudioOutputs)
            {
                mOutputSetting.selectedAudioOutputs.Add(output);
            }
        }

        private void RefreshUseCodec(string codec)
        {
            if (codec == "libx264(H.264)")
                UseH264 = true;
            else
                UseH264 = false;
        }
    }
}
