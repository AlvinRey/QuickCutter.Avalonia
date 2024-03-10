using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using System;

namespace QuickCutter_Avalonia
{
    internal sealed class Program
    {



        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppBuilder appBulider = BuildAvaloniaApp();
            var lifetime = new ClassicDesktopStyleApplicationLifetime()
            {
                Args = args,
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            lifetime.Startup += Lifetime_Startup;
            lifetime.Exit += Lifetime_Exit;
            appBulider.SetupWithLifetime(lifetime);
            lifetime.Start(args);
        }

        private static void Lifetime_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            Config _config = Utils.GetConfig();
            // Set Application Language
            switch (_config.Languages)
            {
                case TextLanguages.ENGLISH:
                    App.SetLanguages("en-US");
                    break;
                case TextLanguages.CHINESE:
                    App.SetLanguages("zh-Hans");
                    break;
            }

            LogHandler.Init();

            // Init FileHandler
            FileHandler.Init(GetStorageProvider());

            try
            {
                FFmpegHandler.CheckFFmpegIsExist();
            }
            catch (Exception ex)
            {
                Utils.ShowNativeMessageBox(ex.Message);
                Environment.Exit(0);
            }
            
        }

        private static void Lifetime_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            ConfigHandler.SaveConfig();
            ExportHandler.CencelWithAppQuit();
            LogHandler.Dispose();
        }


        private static IStorageProvider? GetStorageProvider()
        {
            IClassicDesktopStyleApplicationLifetime? desktop =
                Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var topLevel = TopLevel.GetTopLevel(desktop?.MainWindow);
            return topLevel?.StorageProvider;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                //.UseManagedSystemDialogs()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
