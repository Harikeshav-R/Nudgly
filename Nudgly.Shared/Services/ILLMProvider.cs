using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nudgly.Shared.Models;

namespace Nudgly.Shared.Services;

public interface ILLMProvider
{
    string ProviderId { get; }
    string DisplayName { get; }

    Task<string> GetCompletionAsync(IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default);
}
