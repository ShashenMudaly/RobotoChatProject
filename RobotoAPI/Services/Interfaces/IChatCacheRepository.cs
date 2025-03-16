using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Services.Models;

namespace ChatApp.Services.Interfaces;

public interface IChatCacheRepository
{
    Task StoreMessageInHistory(string userId, string role, string content);
    Task<List<ChatMessage>> GetRecentChatHistory(string userId);
} 