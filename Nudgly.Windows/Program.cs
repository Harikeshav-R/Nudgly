using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nudgly.Shared;
using Nudgly.Shared.Services;
using Nudgly.Windows.Services;

namespace Nudgly.Windows;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        App.ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSingleton<ICaptureExclusionService, WindowsCaptureExclusionService>();
        });

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
