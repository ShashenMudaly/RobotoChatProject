using ChatApp.Services.Interfaces;
using System.Text;
using System;

public class MovieConversationOrchestrator : IMovieConversationOrchestrator
{
    private readonly IChatClient _chatClient;
    private readonly IMovieSearchService _movieSearchService;
    private readonly IChatCacheRepository _cacheRepository;
    private readonly ITextSummarizationService _summarizationService;
    private readonly ILogger<MovieConversationOrchestrator> _logger;

    public MovieConversationOrchestrator(
        IChatClient chatClient,
        IMovieSearchService movieSearchService,
        IChatCacheRepository cacheRepository,
        ITextSummarizationService summarizationService,
        ILogger<MovieConversationOrchestrator> logger)
    {
        _chatClient = chatClient;
        _movieSearchService = movieSearchService;
        _cacheRepository = cacheRepository;
        _summarizationService = summarizationService;
        _logger = logger;
    }

    public async Task<(string response, string context)> ProcessQuery(string userId, string query)
    {
        using var scope = _logger.BeginScope("ProcessQuery for user {UserId}", userId);
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting query processing: {Query}", query);
        var totalDuration = TimeSpan.Zero;
        try
        {
            var recentMessages = await _cacheRepository.GetRecentChatHistory(userId);
            bool isFollowUp = recentMessages.Any();
            _logger.LogInformation("Chat history retrieved in {Duration}ms. Messages: {MessageCount}, IsFollowUp: {IsFollowUp}", 
                (DateTime.UtcNow - startTime).TotalMilliseconds, recentMessages.Count, isFollowUp);

            if (!isFollowUp)
            {
                var intentCheckStart = DateTime.UtcNow;
                var isMovieIntent = await _chatClient.IsIntentForMovieDiscussionUsingAI(query);
                _logger.LogInformation("Intent check completed in {Duration}ms. IsMovieIntent: {IsMovieIntent}",
                    (DateTime.UtcNow - intentCheckStart).TotalMilliseconds, isMovieIntent);

                if (!isMovieIntent)
                {
                    totalDuration = DateTime.UtcNow - startTime;
                    _logger.LogInformation("Query not related to movies. Total duration: {Duration}ms", totalDuration.TotalMilliseconds);
                    return ("I'm a movie assistant. Could you ask me something about movies?", string.Empty);
                }
            }

            var contextBuildStart = DateTime.UtcNow;
            string context = await BuildQueryContext(userId, query, recentMessages);
            _logger.LogInformation("Context built in {Duration}ms. Length: {ContextLength} characters",
                (DateTime.UtcNow - contextBuildStart).TotalMilliseconds, context.Length);

            var responseStart = DateTime.UtcNow;
            var response = await _chatClient.GenerateResponseWithContextAsync(userId, query, context);
            _logger.LogInformation("Response generated in {Duration}ms. Length: {ResponseLength} characters",
                (DateTime.UtcNow - responseStart).TotalMilliseconds, response.Length);

            totalDuration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Query processing completed in {Duration}ms", totalDuration.TotalMilliseconds);
            // Store the interaction in cache
            await _cacheRepository.StoreMessageInHistory(userId, "user", query);
            await _cacheRepository.StoreMessageInHistory(userId, "assistant", response);
            //await _cacheRepository.StoreMessageInHistory(userId, "assistant", context);
            return (response, context);
        }
        catch (Exception ex)
        {
            totalDuration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Error processing query. Total duration: {Duration}ms", totalDuration.TotalMilliseconds);
            throw;
        }
    }

    private async Task<string> BuildQueryContext(string userId, string query, List<ChatMessage> recentMessages)
    {
        var startTime = DateTime.UtcNow;
        var duration = TimeSpan.Zero;
        var movieName = await _chatClient.DetectMovieNameUsingAI(query);
        var movie = await _movieSearchService.LookupMovieByNameAsync(movieName);
        if (movie != null && !string.IsNullOrEmpty(movie.Plot))
        {
        
            _logger.LogInformation("Found movie context for: {MovieName}", movieName);
            return BuildMovieContext(movie, recentMessages);
        }

       
        if (recentMessages.Any())
        {
            var contextCheckStart = DateTime.UtcNow;
            var context = BuildConversationContext(recentMessages);
            
            if (await _chatClient.IsQueryRelatedToExistingContext(query, context))
            {
                duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Using existing context. Duration: {Duration}ms", duration.TotalMilliseconds);
  

            // Iterate through recent messages to find movie references
                foreach (var message in recentMessages.AsEnumerable().Reverse())
                {
                    var messageMovieName = await _chatClient.DetectMovieNameUsingAI(message.Content);
                    if (!string.IsNullOrEmpty(messageMovieName))
                    {
                        var messageMovie = await _movieSearchService.LookupMovieByNameAsync(messageMovieName);
                        if (messageMovie != null && !string.IsNullOrEmpty(messageMovie.Plot))
                        {
                            // Build enhanced context with movie information
                            context = BuildMovieContext(messageMovie, recentMessages);
                            _logger.LogInformation("Found movie context for: {MovieName}", messageMovieName);
                            break; // Stop iteration once we find a valid movie with plot
                        }
                    }
                }
                          return context;
            }
            _logger.LogInformation("Context check completed in {Duration}ms", 
                (DateTime.UtcNow - contextCheckStart).TotalMilliseconds);

        }
        var similarMoviesStart = DateTime.UtcNow;
        var similarMovies = await _movieSearchService.FindSimilarMoviesAsync(query);
        _logger.LogInformation("Similar movies search completed in {Duration}ms. Found: {Count}",
            (DateTime.UtcNow - similarMoviesStart).TotalMilliseconds, similarMovies.Count);

        if (similarMovies.Any())
        {
            return BuildSimilarMoviesContext(similarMovies.Take(3).ToList(), recentMessages);
        }

        duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Falling back to conversation context. Total duration: {Duration}ms", 
            duration.TotalMilliseconds);
        return BuildConversationContext(recentMessages);
    }

    private string BuildMovieContext(MovieSummary movie, List<ChatMessage> history)
    {
        _logger.LogInformation("Building context for movie: {MovieName}", movie.Name);
        var context = new StringBuilder();
        context.AppendLine($"Movie Information:");
        context.AppendLine($"Title: {movie.Name}");
        
        _logger.LogInformation("Summarizing plot for movie: {MovieName}", movie.Name);
        //var summarizedPlot = await _summarizationService.SummarizePlot(movie.Plot);        
        
        context.AppendLine($"Plot: {movie.Plot}");
        
        // context.AppendLine();
        // context.AppendLine("Previous conversation:");
        // AppendChatHistory(context, history);
        
        var result = context.ToString();
        _logger.LogInformation("Movie context built, length: {Length} characters", result.Length);
        return result;
    }

    private string BuildSimilarMoviesContext(List<MovieSummary> movies, List<ChatMessage> history)
    {
        var context = new StringBuilder();
        
        if (movies.Any())
        {
            // Add closest match with full plot
            var closestMatch = movies.First();
            context.AppendLine("Closest movie match:");
            context.AppendLine($"Title: {closestMatch.Name}");
            context.AppendLine($"Plot: {closestMatch.Plot}");
            context.AppendLine();

            // Add other similar movies with summarized plots
            if (movies.Count > 1)
            {
                context.AppendLine("Other similar movies:");
                foreach (var movie in movies.Skip(1))
                {
                    //context.AppendLine($"- {movie.Name}: {movie.Plot}");
                }
            }
        }
        
/*         context.AppendLine();
        context.AppendLine("Previous conversation:");
        AppendChatHistory(context, history); */
        
        var result = context.ToString();
        _logger.LogInformation("Similar movies context built, length: {Length} characters", result.Length);
        return result;
    }

    private string BuildConversationContext(List<ChatMessage> history)
    {
        var context = new StringBuilder();
        context.AppendLine("Previous conversation:");
        AppendChatHistory(context, history);
        return context.ToString();
    }

    private void AppendChatHistory(StringBuilder context, List<ChatMessage> history)
    {
        foreach (var message in history)
        {
            context.AppendLine($"{message.Role}: {message.Content}");
        }
    }
} 