using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Nudgly.Shared.Services;

public interface IScreenCaptureService
{
    Task<Bitmap?> CaptureScreenAsync();
}
