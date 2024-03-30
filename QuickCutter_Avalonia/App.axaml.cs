using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using QuickCutter_Avalonia.ViewModels;
using QuickCutter_Avalonia.Views;
using System;
using System.Linq;




namespace QuickCutter_Avalonia
{
    public partial class App : Application
    {
        private static string m_CurLanguage;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
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
            var translations = Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("/Languages/") ?? false);

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