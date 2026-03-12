using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Nudgly.Windows.Services;

public partial class WindowsScreenCaptureService(ILogger<WindowsScreenCaptureService> logger) : IScreenCaptureService
{
    [LoggerMessage(Level = LogLevel.Information,
        Message = "Capturing virtual screen: Width={Width}, Height={Height}, X={X}, Y={Y}")]
    private partial void LogCapturingScreen(int width, int height, int x, int y);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Screen copied successfully to memory DC.")]
    private partial void LogScreenCopied();

    [LoggerMessage(Level = LogLevel.Information, Message = "Screen capture completed and Avalonia Bitmap generated.")]
    private partial void LogCaptureCompleted();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to capture screen using native APIs.")]
    private partial void LogScreenCaptureFailed(Exception ex);

    public async Task<Bitmap?> CaptureScreenAsync()
    {
        return await Task.Run<Bitmap?>(() =>
        {
            try
            {
                var x = GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);
                var y = GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);
                var width = GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);
                var height = GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);

                LogCapturingScreen(width, height, x, y);

                var writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul);

                try
                {
                    unsafe
                    {
                        using var fb = writeableBitmap.Lock();

                        HDC hdcScreen = default;
                        HDC hdcMem = default;
                        HBITMAP hBitmap = default;
                        HGDIOBJ hOld = default;

                        try
                        {
                            hdcScreen = GetDC();
                            hdcMem = CreateCompatibleDC(hdcScreen);
                            hBitmap = CreateCompatibleBitmap(hdcScreen, width, height);

                            hOld = SelectObject(hdcMem, new HGDIOBJ(hBitmap.Value));

                            // Copy screen to memory DC
                            BitBlt(hdcMem, 0, 0, width, height, hdcScreen, x, y, ROP_CODE.SRCCOPY);

                            LogScreenCopied();

                            var bmpInfo = new BITMAPINFO
                            {
                                bmiHeader = new BITMAPINFOHEADER
                                {
                                    biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                                    biWidth = width,
                                    biHeight = -height, // Negative to indicate top-down DIB
                                    biPlanes = 1,
                                    biBitCount = 32,
                                    biCompression = (uint)BI_COMPRESSION.BI_RGB
                                }
                            };

                            // Get the bits and copy directly to Avalonia's locked frame buffer
                            GetDIBits(
                                hdcMem,
                                hBitmap,
                                0,
                                (uint)height,
                                (void*)fb.Address,
                                &bmpInfo,
                                DIB_USAGE.DIB_RGB_COLORS);
                        }
                        finally
                        {
                            if (hOld.Value != null) SelectObject(hdcMem, hOld);
                            if (hBitmap.Value != null) DeleteObject(new HGDIOBJ(hBitmap.Value));
                            if (hdcMem.Value != null) DeleteDC(hdcMem);
                            if (hdcScreen.Value != null) ReleaseDC(default, hdcScreen);
                        }
                    }

                    LogCaptureCompleted();

                    return writeableBitmap;
                }
                catch
                {
                    writeableBitmap.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                LogScreenCaptureFailed(ex);
                return null;
            }
        });
    }
}
