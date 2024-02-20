using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using System;
using System.Runtime.InteropServices;

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
                ShutdownMode = ShutdownMode.OnLastWindowClose
            };
            appBulider.SetupWithLifetime(lifetime);
            lifetime.Start(args);
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
