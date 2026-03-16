using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Models;

namespace Nudgly.Shared.Services;

public sealed class LLMManagerService : ILLMService
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<LLMManagerService> _logger;

    public IEnumerable<ILLMProvider> AvailableProviders { get; }

    public ILLMProvider CurrentProvider =>
        AvailableProviders.FirstOrDefault(p => p.ProviderId == _appSettings.SelectedProviderId)
        ?? throw new InvalidOperationException($"No provider found with ID '{_appSettings.SelectedProviderId}'");

    public LLMManagerService(
        IEnumerable<ILLMProvider> providers,
        AppSettings appSettings,
        ILogger<LLMManagerService> logger)
    {
        AvailableProviders = providers.ToList();
        _appSettings = appSettings;
        _logger = logger;
    }

    public Task<string> SendMessageAsync(IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending message via {ProviderId}", CurrentProvider.ProviderId);
        return CurrentProvider.GetCompletionAsync(history, cancellationToken);
    }
}
