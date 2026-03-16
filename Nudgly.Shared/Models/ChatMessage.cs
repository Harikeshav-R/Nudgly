using System.Text.Json.Serialization;
using LiveMarkdown.Avalonia;

namespace Nudgly.Shared.Models;

public enum ChatRole
{
    System,
    User,
    Assistant
}

public class ChatMessage
{
    public ChatRole Role { get; set; }
    public string Text { get; set; }
    public string? LocalImagePath { get; set; }

    [JsonIgnore]
    public ObservableStringBuilder MarkdownBuilder { get; }

    public ChatMessage(ChatRole role, string text, string? localImagePath = null)
    {
        Role = role;
        Text = text;
        LocalImagePath = localImagePath;
        MarkdownBuilder = new ObservableStringBuilder();
        MarkdownBuilder.Append(text);
    }

    // Parameterless constructor for JSON deserialization
    public ChatMessage()
    {
        Text = string.Empty;
        MarkdownBuilder = new ObservableStringBuilder();
    }
}
