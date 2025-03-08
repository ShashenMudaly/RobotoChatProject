namespace ChatApp.Services;

using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChatApp.Services.Interfaces;
public class ChatCacheRepository : IChatCacheRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ChatCacheRepository> _logger;

    public ChatCacheRepository(IConnectionMultiplexer redis, ILogger<ChatCacheRepository> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task StoreMessageInHistory(string userId, string role, string content)
    {
        using var scope = _logger.BeginScope("StoreMessageInHistory for user {UserId}", userId);
        _logger.LogInformation("Starting message storage operation");
        var startTime = DateTime.UtcNow;

        try
        {
            var db = _redis.GetDatabase();
            var key = $"chat:history:{userId}";
            
            var message = new ChatMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow,
                MessageId = GenerateMessageId(userId, content)
            };

            _logger.LogDebug("Generated MessageId: {MessageId}", message.MessageId);

            var messages = await db.ListRangeAsync(key, 0, -1);
            var messageJson = JsonSerializer.Serialize(message);

            for (int i = 0; i < messages.Length; i++)
            {
                var existingMessage = JsonSerializer.Deserialize<ChatMessage>(messages[i].ToString());
                if (existingMessage?.MessageId == message.MessageId)
                {
                    await db.ListSetByIndexAsync(key, i, messageJson);
                    _logger.LogInformation("Replaced existing message at index {Index}", i);
                    
                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation("Operation completed in {Duration}ms", duration.TotalMilliseconds);
                    return;
                }
            }

            await db.ListRightPushAsync(key, messageJson);
            await db.ListTrimAsync(key, -10, -1);
            await db.KeyExpireAsync(key, TimeSpan.FromHours(24));
            
            var finalDuration = DateTime.UtcNow - startTime;
            _logger.LogInformation("New message stored. Operation completed in {Duration}ms", finalDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing message");
            throw;
        }
    }

    private string GenerateMessageId(string userId, string content)
    {
        // Create a deterministic ID based on user and content
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes($"{userId}:{content}");
            var hashBytes = sha256.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    public async Task<List<ChatMessage>> GetRecentChatHistory(string userId)
    {
        using var scope = _logger.BeginScope("GetRecentChatHistory for user {UserId}", userId);
        _logger.LogInformation("Starting chat history retrieval");
        var startTime = DateTime.UtcNow;

        try
        {
            var db = _redis.GetDatabase();
            var messages = await db.ListRangeAsync($"chat:history:{userId}", 0, -1);
            
            var result = messages
                .Select(m => JsonSerializer.Deserialize<ChatMessage>(m.ToString()))
                .Where(m => m != null)
                .ToList();

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Retrieved {Count} messages in {Duration}ms", result.Count, duration.TotalMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            throw;
        }
    }
} 