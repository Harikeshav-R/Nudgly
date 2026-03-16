using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Nudgly.Shared.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        var screen = Screens.Primary?.WorkingArea ?? new PixelRect(0, 0, 1920, 1080);
        var x = screen.Center.X - (Bounds.Width / 2);
        var y = screen.Y + 20;

        // Position window top center with a small margin
        Position = new PixelPoint((int)x, y);
    }
}
