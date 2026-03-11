using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Services;

namespace Nudgly.macOS.Services;

public partial class MacOSCaptureExclusionService : ICaptureExclusionService
{
    private readonly ILogger<MacOSCaptureExclusionService> _logger;

    public MacOSCaptureExclusionService(ILogger<MacOSCaptureExclusionService> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not obtain NSWindow for capture exclusion.")]
    private partial void LogMissingHandle();

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying SharingType = None to NSWindow {Handle}")]
    private partial void LogApplyingExclusion(nint handle);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to resolve NSWindow from handle {Handle}")]
    private partial void LogResolveFailure(nint handle);

    [LoggerMessage(Level = LogLevel.Information, Message = "SharingType = None successfully applied.")]
    private partial void LogExclusionSuccess();

    public void ExcludeFromCapture(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle;
        if (handle is null)
        {
            LogMissingHandle();
            return;
        }

        LogApplyingExclusion(handle.Value);

#pragma warning disable CA1416
        var nsWindow = ObjCRuntime.Runtime.GetNSObject<NSWindow>(handle.Value);
#pragma warning restore CA1416
        if (nsWindow is null)
        {
            LogResolveFailure(handle.Value);
            return;
        }

#pragma warning disable CA1416
        nsWindow.SharingType = NSWindowSharingType.None;
#pragma warning restore CA1416
        LogExclusionSuccess();
    }
}
