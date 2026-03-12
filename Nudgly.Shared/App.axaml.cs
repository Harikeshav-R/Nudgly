using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;
using Nudgly.Shared.ViewModels;
using Nudgly.Shared.Views;

namespace Nudgly.Shared;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public static void ConfigureServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        Services = services.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            // DisableAvaloniaDataAnnotationValidation();
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
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

    /*
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    */
}
