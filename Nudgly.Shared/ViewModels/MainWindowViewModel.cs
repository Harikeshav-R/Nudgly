using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nudgly.Shared.Models;
using Nudgly.Shared.Services;
using Nudgly.Shared.Managers;

namespace Nudgly.Shared.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly ILLMService _llmService;
    private readonly IChatManager _chatManager;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _isResponseVisible;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<ChatMessage> ConversationHistory { get; } = new();

    public MainWindowViewModel(
        IScreenCaptureService screenCaptureService,
        ILLMService llmService,
        IChatManager chatManager,
        ILogger<MainWindowViewModel> logger)
    {
        _screenCaptureService = screenCaptureService;
        _llmService = llmService;
        _chatManager = chatManager;
        _logger = logger;

        // Initialize new session completely decoupled from UI thread blocks
        Task.Run(async () => await _chatManager.InitializeNewSessionAsync());
    }

    // Design-time constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindowViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _logger = NullLogger<MainWindowViewModel>.Instance;
    }

    [RelayCommand]
    private async Task AskAIAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            _logger.LogInformation("Capturing screen for Ask AI action.");
            var bitmap = await _screenCaptureService.CaptureScreenAsync();
            string? localImagePath = null;

            if (bitmap is not null)
            {
                var imageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nudgly", "Sessions", "Images");
                Directory.CreateDirectory(imageDir);
                localImagePath = Path.Combine(imageDir, $"capture_{Guid.NewGuid():N}.png");

                // Save asynchronously to avoid blocking UI thread
                await Task.Run(() => bitmap.Save(localImagePath));

                // Free memory now that it's on disk
                bitmap.Dispose();
            }
            else
            {
                _logger.LogWarning("Screen capture returned null.");
            }

            var userText = string.IsNullOrWhiteSpace(UserInput) ? "Describe this screen." : UserInput;
            var userMessage = new ChatMessage(ChatRole.User, userText, localImagePath);

            ConversationHistory.Add(userMessage);
            await _chatManager.AddMessageAsync(userMessage);

            UserInput = string.Empty;

            await FetchLLMResponseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Ask AI execution.");
            var errorMsg = new ChatMessage(ChatRole.Assistant, $"Error: {ex.Message}");
            ConversationHistory.Add(errorMsg);
            await _chatManager.AddMessageAsync(errorMsg);
            IsResponseVisible = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendTextAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(UserInput)) return;

        IsLoading = true;
        try
        {
            var userMessage = new ChatMessage(ChatRole.User, UserInput);
            ConversationHistory.Add(userMessage);
            await _chatManager.AddMessageAsync(userMessage);

            UserInput = string.Empty;

            await FetchLLMResponseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending text message.");
            var errorMsg = new ChatMessage(ChatRole.Assistant, $"Error: {ex.Message}");
            ConversationHistory.Add(errorMsg);
            await _chatManager.AddMessageAsync(errorMsg);
            IsResponseVisible = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FetchLLMResponseAsync()
    {
        IsResponseVisible = true;
        _logger.LogInformation("Sending conversation to LLM...");

        var responseText = await _llmService.SendMessageAsync(ConversationHistory);
        var assistantMessage = new ChatMessage(ChatRole.Assistant, responseText);

        ConversationHistory.Add(assistantMessage);
        await _chatManager.AddMessageAsync(assistantMessage);
    }

    [RelayCommand]
    private void ToggleResponse()
    {
        IsResponseVisible = !IsResponseVisible;
    }
}
