using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace Nudgly.Windows.Services;

#pragma warning disable CA1416 // Validate platform compatibility

public partial class WindowsScreenCaptureService(ILogger<WindowsScreenCaptureService> logger) : IScreenCaptureService
{
    [LoggerMessage(Level = LogLevel.Information,
        Message = "Capturing virtual screen: Width={Width}, Height={Height}, X={X}, Y={Y}")]
    private partial void LogCapturingScreen(int width, int height, int x, int y);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Screen copied successfully. Converting to PNG format.")]
    private partial void LogScreenCopied();

    [LoggerMessage(Level = LogLevel.Information, Message = "Screen capture completed and Avalonia Bitmap generated.")]
    private partial void LogCaptureCompleted();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to capture screen using System.Drawing.")]
    private partial void LogScreenCaptureFailed(Exception ex);

    public Task<AvaloniaBitmap?> CaptureScreenAsync()
    {
        return Task.Run(AvaloniaBitmap? () =>
        {
            try
            {
                var x = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);
                var y = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);
                var width = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);
                var height = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);

                LogCapturingScreen(width, height, x, y);

                using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

                LogScreenCopied();

                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                var avaloniaBitmap = new AvaloniaBitmap(ms);
                LogCaptureCompleted();

                return avaloniaBitmap;
            }
            catch (Exception ex)
            {
                LogScreenCaptureFailed(ex);
                return null;
            }
        });
    }
}
