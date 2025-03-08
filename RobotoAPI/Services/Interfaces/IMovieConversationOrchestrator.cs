namespace ChatApp.Services.Interfaces;

public interface IMovieConversationOrchestrator
{
    Task<(string response, string context)> ProcessQuery(string userId, string query);
} 