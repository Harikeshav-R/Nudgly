using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using System.Net.Http;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Models;
using Nudgly.Shared.Services;
using Nudgly.Shared.ViewModels;
using Nudgly.Shared.Views;
using Nudgly.Shared.Managers;
using LiveMarkdown.Avalonia;

namespace Nudgly.Shared;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public static void ConfigureServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);

        // Add shared services
        services.AddSingleton<AppSettings>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<ILLMProvider, OpenRouterLLMProvider>();
        services.AddSingleton<ILLMService, LLMManagerService>();
        services.AddSingleton<IChatManager, ChatManager>();
        services.AddTransient<MainWindowViewModel>();

        Services = services.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Configure LiveMarkdown
        try
        {
            MarkdownRenderer.ConfigurePipeline += x => x.UseMermaid();
            MarkdownNode.Register<MathInlineNode>();
            MarkdownNode.Register<MathBlockNode>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error configuring LiveMarkdown: " + ex.Message);
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = Services?.GetService<MainWindowViewModel>(),
            };

            Avalonia.Controls.Window.WindowOpenedEvent.AddClassHandler<Avalonia.Controls.Window>(Desktop_WindowOpened);
            desktop.MainWindow = mainWindow;
            desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void Desktop_WindowOpened(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Avalonia.Controls.Window window) return;

        try
        {
            var captureService = Services?.GetService<ICaptureExclusionService>();
            captureService?.ExcludeFromCapture(window);
        }
        catch (Exception ex)
        {
            var logger = Services?.GetService<ILogger<App>>();
            logger?.LogError(ex, "An unexpected error occurred while applying capture exclusion to window {WindowType}", window.GetType().Name);
        }
    }

    private static void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
