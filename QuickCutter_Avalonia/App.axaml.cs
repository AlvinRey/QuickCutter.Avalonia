using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using QuickCutter_Avalonia.ViewModels;
using QuickCutter_Avalonia.Views;
using System;
using Ursa.Controls;

namespace QuickCutter_Avalonia
{
    public partial class App : Application
    {
        //private static Config _config;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //LogHandler.Init();
            //if (ConfigHandler.LoadConfig(ref _config) != 0)


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
            //if (true)
            //{
            //    IClassicDesktopStyleApplicationLifetime? classicDesktopStyleApplicationLifetime = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime;
            //    if(classicDesktopStyleApplicationLifetime != null) 
            //    {
            //        await MessageBox.ShowAsync(classicDesktopStyleApplicationLifetime.MainWindow, "加载GUI配置文件异常,请重启应用", "Error", MessageBoxIcon.Error, MessageBoxButton.OK);
            //    }
                
            //    Environment.Exit(0);
            //}
            base.OnFrameworkInitializationCompleted();
        }
    }
}