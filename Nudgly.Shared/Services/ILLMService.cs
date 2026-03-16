using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nudgly.Shared.Models;

namespace Nudgly.Shared.Services;

public interface ILLMService
{
    IEnumerable<ILLMProvider> AvailableProviders { get; }
    ILLMProvider CurrentProvider { get; }

    Task<string> SendMessageAsync(IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default);
}
