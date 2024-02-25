using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using QuickCutter_Avalonia.ViewModels;
using QuickCutter_Avalonia.Views;
using System;
using System.Linq;
using System.Runtime.InteropServices;



namespace QuickCutter_Avalonia
{
    public partial class App : Application
    {
        private static Config _config;
        private static string m_CurLanguage;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ConfigHandler.LoadConfig(ref _config) != 0)
            {
                // 调用Win32 API显示MessageBox
                MessageBox(IntPtr.Zero, "加载GUI配置文件异常,请重启应用", "Error", 0);
                Environment.Exit(0);
            }

            switch (_config.Languages)
            {
                case TextLanguages.ENGLISH:
                    SetLanguages("en-US");
                    m_CurLanguage = "en-US";
                    break;
                case TextLanguages.CHINESE:
                    SetLanguages("zh-Hans");
                    m_CurLanguage = "zh-Hans";
                    break;
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static void SetLanguages(string targetLanguage)
        {
            if (targetLanguage == m_CurLanguage)
                return;
            var translations = App.Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("/Languages/") ?? false);

            if (translations != null)
                App.Current.Resources.MergedDictionaries.Remove(translations);

            // var resource = AssetLoader.Open(new Uri($"avares://LocalizationSample/Assets/Lang/{targetLanguage}.axaml"));

            App.Current.Resources.MergedDictionaries.Add(
                new ResourceInclude(new Uri($"avares://QuickCutter_Avalonia/Assets/Languages/{targetLanguage}.axaml"))
                {
                    Source = new Uri($"avares://QuickCutter_Avalonia/Assets/Languages/{targetLanguage}.axaml")
                });
            m_CurLanguage = targetLanguage;
        }
    }
}