using System;
using System.Collections.Generic;

namespace Nudgly.Shared.Models;

public class ChatSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; init; } = new();
}
