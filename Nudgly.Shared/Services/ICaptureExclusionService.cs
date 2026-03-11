using Avalonia.Controls;

namespace Nudgly.Shared.Services;

public interface ICaptureExclusionService
{
    void ExcludeFromCapture(Window window);
}
