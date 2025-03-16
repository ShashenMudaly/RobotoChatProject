using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Services.Builders;
using ChatApp.Services.Interfaces;
using ChatApp.Services.Models;
using Microsoft.Extensions.Logging;

namespace ChatApp.Services.Strategies;

public class ConversationStrategy : IContextStrategy
{
    private readonly ILogger<ConversationStrategy> _logger;

    public ConversationStrategy(ILogger<ConversationStrategy> logger)
    {
        _logger = logger;
    }

    public Task<string> BuildContext(object contextData, string query)
    {
        if (contextData is { } data && data.GetType().GetProperty("Messages")?.GetValue(data) is List<ChatMessage> messages)
        {
            return Task.FromResult(BuildFromHistory(messages));
        }
        return Task.FromResult(string.Empty);
    }

    public string BuildFromHistory(List<ChatMessage> history)
    {
        _logger.LogInformation("Building conversation context from {Count} messages", history.Count);
        
        var context = new MovieContextBuilder()
            .AddLine("Previous conversation:");

        foreach (var message in history)
        {
            context.AddLine($"{message.Role}: {message.Content}");
        }

        var result = context.Build();
        _logger.LogInformation("Conversation context built, length: {Length}", result.Length);
        return result;
    }
} 