using Avalonia.Markup.Xaml.Styling;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QuickCutter_Avalonia.ViewModels
{
    public class SettingWindowViewModel : ViewModelBase
    {
        #region private field
        private Config _config;
        #endregion

        public List<WindowStartUpStyles> WindowStartUpStyles { get; set; }
        [Reactive]
        public WindowStartUpStyles SelectedStartUpStyles { get; set; }

        private bool m_SelectEnglish = false;
        public bool SelecteEnglish { get => m_SelectEnglish;
            set
            {
                if (value)
                {
                    Languages = TextLanguages.ENGLISH;
                }
                this.RaiseAndSetIfChanged(ref m_SelectEnglish, value, "SelecteEnglish");
            }
        }

        private bool m_SelecteChinese = false;
        public bool SelecteChinese
        {
            get => m_SelecteChinese;
            set
            {
                if (value)
                {
                    Languages = TextLanguages.CHINESE;
                }
                this.RaiseAndSetIfChanged(ref m_SelecteChinese, value, "SelecteChinese");
            }
        }

        public TextLanguages Languages { get; set; }

        [Reactive]
        public int MoveStep { get; set; }

        public SettingWindowViewModel()
        {
            _config = Utils.GetConfig();
            ReadConfig();

            WindowStartUpStyles = Enum.GetValues(typeof(WindowStartUpStyles)).Cast<WindowStartUpStyles>().ToList();
            switch (Languages)
            {
                case TextLanguages.ENGLISH:
                    m_SelectEnglish = true;
                    break;
                case TextLanguages.CHINESE:
                    m_SelecteChinese = true;
                    break;
            }

        }

        private void ReadConfig()
        {
            SelectedStartUpStyles = _config.windowStartUpStyles;
            MoveStep = _config.moveStep;
            Languages = _config.Languages;
        }

        public void SaveConfig()
        {
            _config.windowStartUpStyles = SelectedStartUpStyles;
            _config.moveStep = MoveStep;
            _config.Languages = Languages;
            OnConfigSaved();
        }

        private void OnConfigSaved()
        {
            switch (_config.Languages)
            {
                case TextLanguages.ENGLISH:
                    App.SetLanguages("en-US");
                    break;
                case TextLanguages.CHINESE:
                    App.SetLanguages("zh-Hans");
                    break;
            }
            
        }
    }
}
