﻿using QuickCutter_Avalonia.Handler;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;

namespace QuickCutter_Avalonia.Models
{
    public partial class OutputFile : ReactiveObject, IDisposable
    {
        #region Private Member
        private CompositeDisposable _subscriptions;
        #endregion

        public ReactiveCommand<Unit, Unit> ReplayCommand { get; }

        [Reactive]
        public int RowIndex { get; set; }

        public string ParentFullName { get; set; }

        public string OutputFileName { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan DefaultOutTime { get; }

        [Reactive]
        public TimeSpan EditInTime { get; set; }

        [Reactive]
        public TimeSpan EditOutTime { get; set; }

        public OutputSetting OutputSetting { get; set; }

        public OutputFile(InputMieda parent)
        {
            var editInTimeChanged = this.WhenAnyValue(v => v.EditInTime);
            var editOutTimeChanged = this.WhenAnyValue(v => v.EditOutTime);
            _subscriptions = new CompositeDisposable
            {
                editInTimeChanged.Subscribe(v =>
                    {
                        if (v <= TimeSpan.Zero)
                            EditInTime = TimeSpan.Zero;
                        if (v >= EditOutTime)
                            EditInTime = EditOutTime - TimeSpan.FromSeconds(1);

                        Duration = (TimeSpan)(EditOutTime - EditInTime);
                    }),
                editOutTimeChanged.Subscribe(v =>
                    {
                        if (v <= EditInTime)
                            EditOutTime = EditInTime + TimeSpan.FromSeconds(1);
                        if (v >= DefaultOutTime)
                            EditOutTime = DefaultOutTime;
                        Duration = (TimeSpan)(EditOutTime - EditInTime);
                    })
            };

            ReplayCommand = ReactiveCommand.Create(() => MediaPlayerHandler.Replay((long)EditInTime.TotalMilliseconds, (long)EditOutTime.TotalMilliseconds));

            ParentFullName = parent.MediaFullName;

            // File Name Setting
            string fileName = System.IO.Path.GetFileNameWithoutExtension(ParentFullName);
            OutputFileName = $"{fileName}_Output_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.mp4";

            // Time Setting
            EditInTime = TimeSpan.Zero;
            DefaultOutTime = parent.Duration;
            EditOutTime = DefaultOutTime;

            OutputSetting = OutputSettingHandler.GenerateOutputSetting(ParentFullName);
            Debug.WriteLine($"{OutputFileName} has OutputSetting @ {OutputSetting.GetHashCode()}");
        }

        ~OutputFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        #region Old Version
        //public List<string> OutputFileExtOptions { get; set; }

        //private string m_OutputFileExt;
        //public string OutputFileExt 
        //{
        //    get => m_OutputFileExt;
        //    set
        //    {
        //        this.RaiseAndSetIfChanged(ref m_OutputFileExt, value, nameof(OutputFileExt));
        //        OnOutputFileExtChanged(value);
        //    }
        //}



        //// Stream
        //[Reactive]
        //public bool HasAudioStream { get; set; } = false;
        //public ObservableCollection<AudioStreamInfo> AudioStreamsInfo { get; set; }


        //[Reactive]
        //public bool HasSubtitleStream { get; set; } = false;
        //[Reactive]
        //public bool AllowMultipleSubtitles { get; set; } = false;
        //public ObservableCollection<SubtitleStreamInfo> SubtitleStreamsInfo { get; set; }

        //[Reactive]
        //public bool IsTransCode { get; set; }

        //[Reactive]
        //public bool UseComstomResolution { get; set; } = false;

        //[Reactive]
        //public int CustomWidth { get; set; }

        //[Reactive]
        //public int CustomHeight { get; set; }

        //public IEnumerable<Codec> CodecOptions { get; } = Utils.GetCodec();

        //[Reactive]
        //public Codec SelectedCodec { get; set; } = VideoCodec.LibX264;



        //// Libx264 AvOption
        //[Reactive]
        //public bool UseLibx264 { get; set; }

        //public IEnumerable<Speed> SpeedPresetOptions { get; } = Enum.GetValues(typeof(Speed)).Cast<Speed>();

        //[Reactive]
        //public Speed SelectedSpeedPreset { get; set; } = Speed.Medium;

        //[Reactive]
        //public int? ConstantRateFactor { get; set; }

        //// Libx265 AvOption
        //[Reactive]
        //public bool UseLibx265 { get; set; }
        //// AV1 AvOption
        //[Reactive]
        //public bool UseAV1 { get; set; }

        ////public OutputFile(InputMieda Parent)
        ////{
        ////    var edit_InTimeChanged = this.WhenAnyValue(v => v.Edit_InTime);
        ////    var edit_OutTimeChanged = this.WhenAnyValue(v => v.Edit_OutTime);
        ////    var selectedCodecChanged = this.WhenAnyValue(v => v.SelectedCodec);
        ////    _subscriptions = new CompositeDisposable
        ////    {
        ////        edit_InTimeChanged.Subscribe(v =>
        ////            {
        ////                if (v <= TimeSpan.Zero)
        ////                    Edit_InTime = TimeSpan.Zero;
        ////                if (v >= Edit_OutTime)
        ////                    Edit_InTime = Edit_OutTime - TimeSpan.FromSeconds(1);

        ////                Duration = (TimeSpan)(Edit_OutTime - Edit_InTime);
        ////            }),
        ////        edit_OutTimeChanged.Subscribe(v =>
        ////            {
        ////                if (v <= Edit_InTime)
        ////                    Edit_OutTime = Edit_InTime + TimeSpan.FromSeconds(1);
        ////                if (v >= Default_OutTime)
        ////                    Edit_OutTime = Default_OutTime;
        ////                Duration = (TimeSpan)(Edit_OutTime - Edit_InTime);
        ////            }),
        ////        selectedCodecChanged.Subscribe(v=>
        ////        {
        ////            if(v.Name == "libx264")
        ////            {
        ////                UseLibx264 = true;
        ////                UseLibx265 = false;
        ////                UseAV1 = false;
        ////            }
        ////            else if(v.Name == "libx265")
        ////            {
        ////                UseLibx264 = false;
        ////                UseLibx265 = true;
        ////                UseAV1 = false;
        ////            }
        ////            else if(v.Name == "av1")
        ////            {
        ////                UseLibx264 = false;
        ////                UseLibx265 = false;
        ////                UseAV1 = true;
        ////            }
        ////        })
        ////    };

        ////    ParentFullName = Parent.MediaFullName;

        ////    // File Name Setting
        ////    string fileName = System.IO.Path.GetFileNameWithoutExtension(ParentFullName);
        ////    OutputFileName = $"{fileName}_Output_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        ////    OutputFileExtOptions = new List<string>
        ////    {
        ////        new string(".mp4"),
        ////        new string(".mkv")
        ////    };
        ////    OutputFileExt = OutputFileExtOptions.First();

        ////    // Time Setting
        ////    Edit_InTime = TimeSpan.Zero;
        ////    Default_OutTime = Parent.Duration;
        ////    Edit_OutTime = Default_OutTime;


        ////    

        ////    // Resolution Setting
        ////    DefaultHeight = Parent.Height;
        ////    DefaultWidth = Parent.Width;
        ////    CustomWidth = DefaultWidth;
        ////    CustomHeight = DefaultHeight;

        ////    // CRF Setting
        ////    ConstantRateFactor = 23;

        ////    // Codec Setting

        ////    SelectedCodec = VideoCodec.LibX264;

        ////    ReplayCommand = ReactiveCommand.Create(() => MediaPlayerHandler.Replay((long)Edit_InTime.TotalMilliseconds, (long)Edit_OutTime.TotalMilliseconds));
        ////}

        //private void OnOutputFileExtChanged(string NewValue)
        //{
        //    if (NewValue == ".mkv")
        //    {
        //        AllowMultipleSubtitles = true;
        //    }
        //    else
        //    {
        //        AllowMultipleSubtitles = false;
        //        var lst = SubtitleStreamsInfo?.Where(v => v.IsMapped == true);
        //        if(lst!=null)
        //        {
        //            foreach (var s in lst)
        //            {
        //                s.IsMapped = false;
        //            }
        //        }
        //    }
        //}
        #endregion
    }
}
