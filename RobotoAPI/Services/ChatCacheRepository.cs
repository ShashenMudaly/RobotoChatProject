using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChatApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using ChatApp.Services.Models;

namespace ChatApp.Services;

public class ChatCacheRepository : IChatCacheRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ChatCacheRepository> _logger;
    private const string ChatHistoryPrefix = "chat:history:";
    private const int MaxHistoryItems = 10;

    public ChatCacheRepository(IConnectionMultiplexer redis, ILogger<ChatCacheRepository> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task StoreMessageInHistory(string userId, string role, string content)
    {
        try
        {
            var message = new ChatMessage(role, content);
            var db = _redis.GetDatabase();
            var key = $"{ChatHistoryPrefix}{userId}";

            var serializedMessage = JsonSerializer.Serialize(message);
            await db.ListLeftPushAsync(key, serializedMessage);
            await db.ListTrimAsync(key, 0, MaxHistoryItems - 1);

            _logger.LogInformation("Stored message in history for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing message in history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ChatMessage>> GetRecentChatHistory(string userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{ChatHistoryPrefix}{userId}";
            var messages = await db.ListRangeAsync(key);

            if (!messages.Any())
            {
                _logger.LogInformation("No chat history found for user {UserId}", userId);
                return new List<ChatMessage>();
            }

            var history = messages
                .Select(m => JsonSerializer.Deserialize<ChatMessage>(m.ToString()))
                .Where(m => m != null)
                .Select(m => m!)
                .OrderBy(m => m.Timestamp)
                .ToList();

            _logger.LogInformation("Retrieved {Count} messages from history for user {UserId}", 
                history.Count, userId);

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history for user {UserId}", userId);
            throw;
        }
    }
} 