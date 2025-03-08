public interface IChatClient
{
    Task<string> GenerateResponseWithContextAsync(string userId, string query, string context);
    Task<string> DetectMovieNameUsingAI(string query);
    Task<bool> IsIntentForMovieDiscussionUsingAI(string query);
    Task<bool> IsQueryRelatedToExistingContext(string query, string context);
} 