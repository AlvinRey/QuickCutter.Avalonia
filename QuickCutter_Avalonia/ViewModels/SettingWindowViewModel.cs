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

        private WindowStartUpStyles m_SelectedStartUpStyles;
        public int WindowStartUpStylesComboBoxSelectedIndex {
            get
            {
                return (int)m_SelectedStartUpStyles - 1;
            } 
            set
            {
                this.RaiseAndSetIfChanged(ref m_SelectedStartUpStyles, (WindowStartUpStyles)(value + 1), "WindowStartUpStylesComboBoxSelectedIndex");
            }
        }

        private TextLanguages m_Languages;
        public int LanguageComboBoxSelectedIndex
        {
            get
            {
                return (int)m_Languages - 1;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref m_Languages, (TextLanguages)(value + 1), "LanguageComboBoxSelectedIndex");
            }
        }

        [Reactive]
        public bool AutoPlay { get; set; }

        [Reactive]
        public int MoveStep { get; set; }

        public SettingWindowViewModel()
        {
            _config = Utils.GetConfig();
            ReadConfig();
        }

        private void ReadConfig()
        {
            m_SelectedStartUpStyles = _config.windowStartUpStyles;
            m_Languages = _config.Languages;

            MoveStep = _config.moveStep;
            AutoPlay = _config.autoPlay;
        }

        public void SaveConfig()
        {
            _config.windowStartUpStyles = m_SelectedStartUpStyles;
            _config.Languages = m_Languages;

            _config.autoPlay = AutoPlay;
            _config.moveStep = MoveStep;
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
