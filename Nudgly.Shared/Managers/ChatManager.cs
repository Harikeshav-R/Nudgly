using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nudgly.Shared.Models;

namespace Nudgly.Shared.Managers;

public interface IChatManager
{
    ChatSession CurrentSession { get; }
    Task InitializeNewSessionAsync();
    Task AddMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatSession>> GetAllSessionsAsync();
}

public sealed class ChatManager : IChatManager
{
    private readonly ILogger<ChatManager> _logger;
    private readonly string _sessionsDirectory;

    public ChatSession CurrentSession { get; private set; } = new();

    public ChatManager(ILogger<ChatManager> logger)
    {
        _logger = logger;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _sessionsDirectory = Path.Combine(appData, "Nudgly", "Sessions");
        Directory.CreateDirectory(_sessionsDirectory);
    }

    public Task InitializeNewSessionAsync()
    {
        CurrentSession = new ChatSession();
        _logger.LogInformation("Initialized new chat session {SessionId}", CurrentSession.Id);
        return Task.CompletedTask;
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        CurrentSession.Messages.Add(message);
        await SaveSessionAsync(CurrentSession);
    }

    private async Task SaveSessionAsync(ChatSession session)
    {
        try
        {
            var filePath = Path.Combine(_sessionsDirectory, $"{session.Id}.json");

            // We use options to ensure we don't serialize the MarkdownBuilder, which is UI-specific
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // Only serialize public properties, ignore getters that throw (if any)
                IgnoreReadOnlyProperties = false
            };

            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await JsonSerializer.SerializeAsync(stream, session, options);

            _logger.LogDebug("Saved chat session {SessionId} to disk", session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save chat session {SessionId}", session.Id);
        }
    }

    public async Task<IEnumerable<ChatSession>> GetAllSessionsAsync()
    {
        var sessions = new List<ChatSession>();

        try
        {
            var files = Directory.GetFiles(_sessionsDirectory, "*.json");
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            foreach (var file in files)
            {
                try
                {
                    await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                    var session = await JsonSerializer.DeserializeAsync<ChatSession>(stream, options);
                    if (session != null)
                    {
                        sessions.Add(session);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize session file {FilePath}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read sessions directory");
        }

        sessions.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt)); // Newest first
        return sessions;
    }
}
