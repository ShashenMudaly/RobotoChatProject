using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Services.Models;

namespace ChatApp.Services.Interfaces;

public interface IChatClient
{
    Task<string> GenerateResponseWithContextAsync(string userId, string query, string context);
    Task<string> DetectMovieNameUsingAI(string userInput);
    Task<string> DetectMovieNameUsingAI(List<string> messages);
    Task<bool> IsIntentForMovieDiscussionUsingAI(string userInput);
    Task<bool> IsIntentForMovieDiscussionUsingAI(string userInput, List<ChatMessage>? conversationHistory);
    Task<bool> IsQueryRelatedToExistingContext(string query, string context);
    Task<bool> CanAnswerFromContext(string query, string context);
} 