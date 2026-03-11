using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Nudgly.Windows.Services;

public partial class WindowsCaptureExclusionService : ICaptureExclusionService
{
    private readonly ILogger<WindowsCaptureExclusionService> _logger;

    public WindowsCaptureExclusionService(ILogger<WindowsCaptureExclusionService> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not obtain HWND for capture exclusion.")]
    private partial void LogMissingHandle();

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying WDA_EXCLUDEFROMCAPTURE to HWND {Hwnd}")]
    private partial void LogApplyingExclusion(nint hwnd);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to apply WDA_EXCLUDEFROMCAPTURE to HWND {Hwnd}. Error code: {ErrorCode}")]
    private partial void LogExclusionFailure(nint hwnd, int errorCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "WDA_EXCLUDEFROMCAPTURE successfully applied.")]
    private partial void LogExclusionSuccess();

    public void ExcludeFromCapture(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle;
        if (handle is null)
        {
            LogMissingHandle();
            throw new InvalidOperationException("Could not obtain HWND");
        }

        var hwnd = (HWND)handle.Value;
        LogApplyingExclusion(handle.Value);

        if (!PInvoke.SetWindowDisplayAffinity(hwnd, WINDOW_DISPLAY_AFFINITY.WDA_EXCLUDEFROMCAPTURE))
        {
            var error = Marshal.GetLastWin32Error();
            LogExclusionFailure(handle.Value, error);
            throw new Win32Exception(error);
        }

        LogExclusionSuccess();
    }
}
