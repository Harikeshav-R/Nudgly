using System;

namespace Nudgly.Shared.Models;

public sealed record AppSettings
{
    // In a real app, this would be loaded from a config file.
    public string SelectedProviderId { get; set; } = "openrouter";
    public string OpenRouterApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? string.Empty;
}
