using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Avalonia.Media.Imaging;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;
using ScreenCaptureKit;

namespace Nudgly.macOS.Services;

public partial class MacOSScreenCaptureService : IScreenCaptureService
{
    private readonly ILogger<MacOSScreenCaptureService> _logger;
    private static readonly NSDictionary EmptyDictionary = new();

    public MacOSScreenCaptureService(ILogger<MacOSScreenCaptureService> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Capturing primary display: Width={Width}, Height={Height}")]
    private partial void LogCapturingScreen(int width, int height);

    [LoggerMessage(Level = LogLevel.Information, Message = "Screen capture completed and Avalonia Bitmap generated.")]
    private partial void LogCaptureCompleted();

    [LoggerMessage(Level = LogLevel.Warning, Message = "No displays found for screen capture.")]
    private partial void LogNoDisplaysFound();

    [LoggerMessage(Level = LogLevel.Warning, Message = "ScreenCaptureKit returned a null image.")]
    private partial void LogNullImageReturned();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to convert captured image to TIFF.")]
    private partial void LogTiffConversionFailed();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to convert captured image to PNG.")]
    private partial void LogPngConversionFailed();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to capture screen using ScreenCaptureKit.")]
    private partial void LogScreenCaptureFailed(Exception ex);

    public async Task<Bitmap?> CaptureScreenAsync()
    {
        try
        {
            var content = await SCShareableContent.GetShareableContentAsync(false, true);
            var display = content.Displays.FirstOrDefault();

            if (display == null)
            {
                LogNoDisplaysFound();
                return null;
            }

            LogCapturingScreen((int)display.Width, (int)display.Height);

            // Exclude empty array of windows to capture the whole display
            var filter = new SCContentFilter(display, Array.Empty<SCWindow>(), SCContentFilterOption.Exclude);
            var config = new SCStreamConfiguration
            {
                Width = (nuint)display.Width,
                Height = (nuint)display.Height,
                ShowsCursor = false
            };

            using var cgImage = await SCScreenshotManager.CaptureImageAsync(filter, config);
            if (cgImage == null)
            {
                LogNullImageReturned();
                return null;
            }

            using var nsImage = new NSImage(cgImage, new CGSize(cgImage.Width, cgImage.Height));
            using var tiffData = nsImage.AsTiff();
            if (tiffData == null)
            {
                LogTiffConversionFailed();
                return null;
            }

            using var bitmapRep = new NSBitmapImageRep(tiffData);
            using var pngData = bitmapRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, EmptyDictionary);

            if (pngData == null)
            {
                LogPngConversionFailed();
                return null;
            }

            using var stream = new MemoryStream(pngData.ToArray());
            var bitmap = new Bitmap(stream);
            LogCaptureCompleted();
            return bitmap;
        }
        catch (Exception ex)
        {
            LogScreenCaptureFailed(ex);
            return null;
        }
    }
}
