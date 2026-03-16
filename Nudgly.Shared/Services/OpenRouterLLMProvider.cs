using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Models;

namespace Nudgly.Shared.Services;

public sealed class OpenRouterLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _appSettings;
    private readonly ILogger<OpenRouterLLMProvider> _logger;
    private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string DefaultModel = "google/gemini-2.5-flash";

    public string ProviderId => "openrouter";
    public string DisplayName => "OpenRouter";

    public OpenRouterLLMProvider(
        HttpClient httpClient,
        AppSettings appSettings,
        ILogger<OpenRouterLLMProvider> logger)
    {
        _httpClient = httpClient;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_appSettings.OpenRouterApiKey))
        {
            throw new InvalidOperationException("OpenRouter API key is not configured.");
        }

        var messages = new List<OpenRouterMessage>();

        messages.Add(new OpenRouterMessage(
            "system",
            new List<OpenRouterContentPart>
            {
                new OpenRouterContentPart("text", Text: "If you do not know something, ask the user or state that you do not know. DO NOT HALLUCINATE.")
            }
        ));

        foreach (var msg in history)
        {
            var role = msg.Role switch
            {
                ChatRole.System => "system",
                ChatRole.User => "user",
                ChatRole.Assistant => "assistant",
                _ => "user"
            };

            var parts = new List<OpenRouterContentPart>();
            if (!string.IsNullOrWhiteSpace(msg.Text))
            {
                parts.Add(new OpenRouterContentPart("text", Text: msg.Text));
            }

            if (!string.IsNullOrWhiteSpace(msg.LocalImagePath) && System.IO.File.Exists(msg.LocalImagePath))
            {
                var imageBytes = await System.IO.File.ReadAllBytesAsync(msg.LocalImagePath, cancellationToken);
                var base64 = Convert.ToBase64String(imageBytes);
                parts.Add(new OpenRouterContentPart("image_url", ImageUrl: new OpenRouterImageUrl($"data:image/png;base64,{base64}")));
            }

            messages.Add(new OpenRouterMessage(role, parts));
        }

        var requestBody = new OpenRouterRequest(DefaultModel, messages);

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.OpenRouterApiKey);
        request.Content = JsonContent.Create(requestBody, OpenRouterJsonContext.Default.OpenRouterRequest);

        _logger.LogInformation("Sending request to OpenRouter API...");
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenRouter API error: {StatusCode} {ErrorDetails}", response.StatusCode, errorDetails);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync(OpenRouterJsonContext.Default.OpenRouterResponse, cancellationToken);
        var content = result?.Choices?[0]?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Received empty or null completion from OpenRouter.");
            return "No response from AI.";
        }

        return content;
    }
}
