using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QuickCutter_Avalonia.ViewModels
{
    internal class SettingWindowViewModel : ViewModelBase
    {
        #region private field
        private Config _config;
        #endregion

        public List<WindowStartUpStyles> WindowStartUpStyles { get; set; }
        [Reactive]
        public WindowStartUpStyles SelectedStartUpStyles { get; set; }

        public SettingWindowViewModel() 
        {
            _config = Utils.GetConfig();
            Debug.WriteLine(_config.GetHashCode().ToString());
            WindowStartUpStyles = Enum.GetValues(typeof(WindowStartUpStyles)).Cast<WindowStartUpStyles>().ToList();
            SelectedStartUpStyles = _config.windowStartUpStyles;
        }

        public void SaveConfig()
        {
            _config.windowStartUpStyles = SelectedStartUpStyles;
        }
    }
}
