using FFMpegCore.Enums;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace QuickCutter_Avalonia.ViewModels
{
    public class OutputSettingViewModel : ViewModelBase
    {
        private OutputSetting? _outputSetting;

        #region Video Setting
        public IEnumerable<string> VideoCodecOptions
        {
            get => OutputSettingHandler.VideoCodecs;
        }

        public string SelectedVideoCodec
        {
            get
            {
                if (_outputSetting is null) return null;
                return _outputSetting.videoSetting.selectedVideoCodec;
            }

            set
            {
                if (_outputSetting is null) return;
                this.RaiseAndSetIfChanged(ref _outputSetting.videoSetting.selectedVideoCodec, value, 
                    nameof(SelectedVideoCodec));
                OnCodecChanged(value);
            }
        }

        public bool UseNetworkOptimization
        {
            get
            {
                if (_outputSetting is null) return false;
                return _outputSetting.videoSetting.useNetworkOptimization;
            }
            set
            {
                if(_outputSetting is null) return;
                this.RaiseAndSetIfChanged(ref _outputSetting.videoSetting.useNetworkOptimization, value,
                    nameof(UseNetworkOptimization));
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
                if (_outputSetting is null) return Speed.Fast;
                return _outputSetting.videoSetting.selectedSpeedPreset;
            }

            set
            {
                if (_outputSetting is null) return;
                this.RaiseAndSetIfChanged(ref _outputSetting.videoSetting.selectedSpeedPreset, value, nameof(SelectedSpeedPreset));
            }
        }

        public int ConstantRateFactor
        {
            get
            {
                if (_outputSetting is null) return 23;
                return _outputSetting.videoSetting.constantRateFactor;
            }

            set
            {
                if (_outputSetting is null) return;
                this.RaiseAndSetIfChanged(ref _outputSetting.videoSetting.constantRateFactor, value, nameof(ConstantRateFactor));
            }
        }
        #endregion

        #region Audio Output

        public ReactiveCommand<Unit, Unit> ClearAudioOutputCommand { get; set; }

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
                if (_outputSetting is null) return false;
                return _outputSetting.isBurnSubtitle;
            }

            set
            {
                if (_outputSetting is null) return;
                this.RaiseAndSetIfChanged(ref _outputSetting.isBurnSubtitle, value, nameof(BurnSubtitle));
                NotifyViewModelsUpdate();
            }
        }

        public ReactiveCommand<Unit, Unit> ClearSubtitleOutputCommand { get; set; }

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
            _outputSetting = outputSetting;
            BuildAudioStreamOptions();
            BuildSubtitleStreamOptions();
            RefreshDisplay();
        }

        public void UnLoadDisplayOutputSetting()
        {
            _outputSetting = null;
            AudioStreamOptions.Clear();
            SubtitleStreamOptions.Clear();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            this.RaisePropertyChanged(nameof(SelectedVideoCodec));
            this.RaisePropertyChanged(nameof(UseNetworkOptimization));
            this.RaisePropertyChanged(nameof(SelectedSpeedPreset));
            this.RaisePropertyChanged(nameof(ConstantRateFactor));
            this.RaisePropertyChanged(nameof(HasAudio));
            this.RaisePropertyChanged(nameof(HasSubtitle));
            this.RaisePropertyChanged(nameof(BurnSubtitle));
            OnCodecChanged(SelectedVideoCodec);
        }

        private void BuildSubtitleStreamOptions()
        {
            if (_outputSetting == null) return;

            foreach (var subtitleStream in OutputSettingHandler.SubtitleStreamDictonary[_outputSetting.key])
            {
                SubtitleStreamOptions.Add(new SelecteSubtitleStreamViewModel(subtitleStream, _outputSetting.selectedSubtitleOutputs, NotifyViewModelsUpdate));
            }
            NotifyViewModelsUpdate();
        }

        private void BuildAudioStreamOptions()
        {
            if (_outputSetting == null) return;

            foreach (var audioStream in OutputSettingHandler.AudioStreamDictonary[_outputSetting.key])
            {
                AudioStreamOptions.Add(new SelecteAudioStreamViewModel(audioStream, _outputSetting.selectedAudioOutputs));
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
            var firstSelectedVm = SubtitleStreamOptions.FirstOrDefault(vm => vm.IsSelected == true);
            bool? isSelectTextType = firstSelectedVm is null ? null : firstSelectedVm.IsTextType;

            foreach (var vm in SubtitleStreamOptions)
            {
                vm.UpdateCanSelectState(BurnSubtitle, isSelectTextType, firstSelectedVm);
            }
        }
    }
}
