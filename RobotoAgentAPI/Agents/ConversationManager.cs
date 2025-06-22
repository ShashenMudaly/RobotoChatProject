using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;

namespace RobotoAgentAPI.Agents;

public class ConversationManager
{
    private readonly ConcurrentDictionary<string, ChatHistory> _userConversations;
    private readonly int _maxHistoryLength = 20; // Maximum number of message pairs to keep

    public ConversationManager()
    {
        _userConversations = new ConcurrentDictionary<string, ChatHistory>();
    }

    public ChatHistory GetOrCreateUserConversation(string userId, string systemPrompt)
    {
        return _userConversations.GetOrAdd(userId, _ => 
        {
            var newHistory = new ChatHistory();
            newHistory.AddSystemMessage(systemPrompt);
            Console.WriteLine($"Created new conversation history for user: {userId}");
            return newHistory;
        });
    }

    public void AddToConversation(string userId, string userMessage, string assistantResponse)
    {
        if (_userConversations.TryGetValue(userId, out var chatHistory))
        {
            chatHistory.AddUserMessage(userMessage);
            chatHistory.AddAssistantMessage(assistantResponse);
            TrimConversationHistory(chatHistory);
        }
    }

    public string ClearConversationHistory(string userId)
    {
        if (_userConversations.TryRemove(userId, out _))
        {
            Console.WriteLine($"Cleared conversation history for user: {userId}");
            return "Conversation history cleared. How can I help you with movies today?";
        }
        
        return "No conversation history found to clear.";
    }

    public Task<string> GetConversationSummaryAsync(string userId)
    {
        if (!_userConversations.TryGetValue(userId, out var chatHistory) || !chatHistory.Any())
        {
            return Task.FromResult("No conversation history found.");
        }

        try
        {
            var messageCount = chatHistory.Count - 1; // Exclude system message
            var recentMessages = chatHistory.Skip(Math.Max(1, chatHistory.Count - 6)).Take(5);
            
            var summary = $"Conversation contains {messageCount} messages. Recent topics discussed: ";
            var topics = recentMessages
                .Where(m => m.Role == AuthorRole.User)
                .Select(m => m.Content?.Length > 50 ? m.Content.Substring(0, 50) + "..." : m.Content)
                .Where(content => !string.IsNullOrEmpty(content));
                
            summary += string.Join(", ", topics);
            
            return Task.FromResult(summary);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating conversation summary: {ex.Message}");
            return Task.FromResult("Unable to generate conversation summary.");
        }
    }

    public string GetConversationContext(ChatHistory chatHistory, int maxMessages = 10, bool includeSystemPrompt = true)
    {
        var messages = new List<string>();
        var startIndex = includeSystemPrompt ? 0 : 1; // Skip system message if not needed
        var messagesToInclude = Math.Min(maxMessages, chatHistory.Count - startIndex);
        
        for (int i = Math.Max(startIndex, chatHistory.Count - messagesToInclude); i < chatHistory.Count; i++)
        {
            var message = chatHistory[i];
            var role = message.Role == AuthorRole.User ? "User" : 
                      message.Role == AuthorRole.Assistant ? "Assistant" : 
                      "System";
            
            if (message.Role == AuthorRole.System && !includeSystemPrompt)
                continue;
                
            messages.Add($"{role}: {message.Content}");
        }
        
        return string.Join("\n", messages);
    }

    private void TrimConversationHistory(ChatHistory chatHistory)
    {
        // Keep system message + last N message pairs (user + assistant)
        // Each pair = 2 messages, so max total = 1 system + (N * 2) messages
        var maxTotalMessages = 1 + (_maxHistoryLength * 2);
        
        while (chatHistory.Count > maxTotalMessages)
        {
            // Remove the oldest non-system message (index 1, since system is at index 0)
            if (chatHistory.Count > 1)
            {
                chatHistory.RemoveAt(1);
            }
        }
        
        Console.WriteLine($"Conversation history trimmed to {chatHistory.Count} messages");
    }
} 