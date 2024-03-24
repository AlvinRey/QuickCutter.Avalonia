using FFMpegCore.Enums;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

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
                OnCodecChanged(value);
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
            get => AudioStreamOptions.Count > 0;
        }

        public ObservableCollection<SelecteAudioStreamViewModel> AudioStreamOptions { get; set; }
        #endregion

        #region Subtitle Output
        [Reactive]
        public bool CanBurn { get; set; }

        public bool BurnSubtitle
        {
            get
            {
                if (mOutputSetting is null) return false;
                return mOutputSetting.isBurnSubtitle;
            }

            set
            {
                if (mOutputSetting is null) return;
                this.RaiseAndSetIfChanged(ref mOutputSetting.isBurnSubtitle, value, nameof(BurnSubtitle));
                NotifyViewModelsUpdate();
            }
        }

        public IReactiveCommand ClearSubtitleOutputCommand { get; set; }

        public bool HasSubtitle
        {
            get => SubtitleStreamOptions.Count > 0;
        }

        public ObservableCollection<SelecteSubtitleStreamViewModel> SubtitleStreamOptions { get; set; }

        #endregion

        public OutputSettingViewModel()
        {
            AudioStreamOptions = new ObservableCollection<SelecteAudioStreamViewModel>();
            SubtitleStreamOptions = new ObservableCollection<SelecteSubtitleStreamViewModel>();

            ClearAudioOutputCommand = ReactiveCommand.Create(() =>
            {
                foreach (var audioSteam in AudioStreamOptions)
                {
                    if (audioSteam.IsSelected)
                    {
                        audioSteam.IsSelected = false;
                    }
                }
            });

            ClearSubtitleOutputCommand = ReactiveCommand.Create(() =>
            {
                foreach (var subtitleStream in SubtitleStreamOptions)
                {
                    if (subtitleStream.IsSelected)
                    {
                        subtitleStream.IsSelected = false;
                    }
                }
            });
        }

        public void LoadDisplayOutputSetting(OutputSetting outputSetting)
        {
            mOutputSetting = outputSetting;
            BuildAudioStreamOptions();
            BuildSubtitleStreamOptions();
            RefreshDisplay();
        }

        public void UnLoadDisplayOutputSetting()
        {
            mOutputSetting = null;
            AudioStreamOptions.Clear();
            SubtitleStreamOptions.Clear();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            this.RaisePropertyChanged(nameof(SelectedVideoCodec));
            this.RaisePropertyChanged(nameof(SelectedSpeedPreset));
            this.RaisePropertyChanged(nameof(ConstantRateFactor));
            this.RaisePropertyChanged(nameof(HasAudio));
            this.RaisePropertyChanged(nameof(HasSubtitle));
            this.RaisePropertyChanged(nameof(BurnSubtitle));
            OnCodecChanged(SelectedVideoCodec);
        }

        private void BuildSubtitleStreamOptions()
        {
            if (mOutputSetting == null) return;

            foreach (var subtitleStream in OutputSettingHandler.SubtitleStreamDictonary[mOutputSetting.key])
            {
                SubtitleStreamOptions.Add(new SelecteSubtitleStreamViewModel(subtitleStream, mOutputSetting.selectedSubtitleOutputs, NotifyViewModelsUpdate));
            }
            NotifyViewModelsUpdate();
        }

        private void BuildAudioStreamOptions()
        {
            if (mOutputSetting == null) return;

            foreach (var audioStream in OutputSettingHandler.AudioStreamDictonary[mOutputSetting.key])
            {
                AudioStreamOptions.Add(new SelecteAudioStreamViewModel(audioStream, mOutputSetting.selectedAudioOutputs));
            }
        }

        private void OnCodecChanged(string codec)
        {
            if (codec == "Copy")
            {
                if (BurnSubtitle)
                {
                    BurnSubtitle = false;
                }
                CanBurn = false;
                UseH264 = false;
            }
            else if (codec == "Libx264")
            {
                CanBurn = true;
                UseH264 = true;
            }
            else if (codec == "Libx265")
            {
                CanBurn = true;
                UseH264 = false;
            }
        }

        private void NotifyViewModelsUpdate()
        {
            var firstSelectedVM = SubtitleStreamOptions.FirstOrDefault(vm => vm.IsSelected == true);
            bool? isSelectTextType = firstSelectedVM is null ? null : firstSelectedVM.IsTextType;

            foreach (var vm in SubtitleStreamOptions)
            {
                vm.UpdateCanSelectState(BurnSubtitle, isSelectTextType, firstSelectedVM);
            }
        }
    }
}
