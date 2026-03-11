using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;
using ScreenCaptureKit;

namespace Nudgly.macOS.Services;

public partial class MacOsScreenCaptureService(ILogger<MacOsScreenCaptureService> logger) : IScreenCaptureService
{
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

            // Exclude empty array of windows to capture the whole display
            var filter = new SCContentFilter(display, Array.Empty<SCWindow>(), SCContentFilterOption.Exclude);
            var config = new SCStreamConfiguration
            {
                Width = (nuint)display.Width,
                Height = (nuint)display.Height,
                ShowsCursor = false
            };

            var cgImage = await SCScreenshotManager.CaptureImageAsync(filter, config);
            if (cgImage == null)
            {
                LogNullImageReturned();
                return null;
            }

            var nsImage = new NSImage(cgImage, new CGSize(cgImage.Width, cgImage.Height));
            var tiffData = nsImage.AsTiff();
            if (tiffData == null)
            {
                LogTiffConversionFailed();
                return null;
            }

            var bitmapRep = new NSBitmapImageRep(tiffData);
            var pngData = bitmapRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, new NSDictionary());

            if (pngData == null)
            {
                LogPngConversionFailed();
                return null;
            }

            using var stream = new MemoryStream(pngData.ToArray());
            return new Bitmap(stream);
        }
        catch (Exception ex)
        {
            LogScreenCaptureFailed(ex);
            return null;
        }
    }
}
