using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
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
                ShutdownMode = ShutdownMode.OnLastWindowClose
            };
            //lifetime.Exit += OnExit;
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

        //static void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        //{

        //}
    }
}
