using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursa.Controls;

namespace QuickCutter_Avalonia.Models
{
    public class StreamInfo : ViewModelBase
    {
        // name format: title-[language]
        public string Name { get; set; }

        public int AbsoluteStreamIndex { get; set; }

        public int RelativeStreamIndex { get; set; }

        private bool m_IsMapped;

        public bool IsMapped
        {
            get
            {
                return m_IsMapped;
            }
            set
            {
                if (!value | CanSelect.Invoke())
                {
                    this.RaiseAndSetIfChanged(ref m_IsMapped, value, "Mapped");
                }
                else
                {
                    MessageBox.ShowAsync("Mp4格式只能选择一个字幕进行烧录", "Notice", MessageBoxIcon.Information, MessageBoxButton.OK).Await();
                }
            }
        }
        public Func<bool> CanSelect { get; set; }

        [Reactive]
        public bool IsEnable { get; set; } = true;
    }

    //public class AudioStreamInfo : StreamInfo
    //{

    //}

    //public class SubtitleStreamInfo : StreamInfo
    //{
    //    public bool IsTextType { get; set; }
    //}
}
