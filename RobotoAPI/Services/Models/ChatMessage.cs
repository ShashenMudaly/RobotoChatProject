using System;

namespace ChatApp.Services.Models;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
        Timestamp = DateTime.UtcNow;
    }

    public ChatMessage()
    {
        Timestamp = DateTime.UtcNow;
    }
} 