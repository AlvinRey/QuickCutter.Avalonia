using QuickCutter_Avalonia.ViewModels;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Models
{
    public class StreamInfo : ViewModelBase
    {
        // name format: title-[language]
        public string Name { get; set; }
        [Reactive]
        public bool Mapped { get; set; } = true;

        public int AbsoluteStreamIndex { get; set; }
    }
}
