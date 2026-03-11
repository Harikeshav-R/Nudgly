using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nudgly.Shared;
using Nudgly.Shared.Services;
using Nudgly.macOS.Services;

namespace Nudgly.macOS;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#pragma warning disable CA1416
        NSApplication.Init();
#pragma warning restore CA1416

        App.ConfigureServices(services =>
        {
            services.AddLogging(builder =>
            {
#if DEBUG
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
#endif
            });
            services.AddSingleton<ICaptureExclusionService, MacOSCaptureExclusionService>();
        });

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new MacOSPlatformOptions
            {
                ShowInDock = false
            })
            .LogToTrace();
}
