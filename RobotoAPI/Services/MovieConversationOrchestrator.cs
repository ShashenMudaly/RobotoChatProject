using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Extensions;
using ChatApp.Services.Factories;
using ChatApp.Services.Interfaces;
using ChatApp.Services.Models;
using Microsoft.Extensions.Logging;

namespace ChatApp.Services;

public class MovieConversationOrchestrator : IMovieConversationOrchestrator
{
    private readonly IChatClient _chatClient;
    private readonly IMovieSearchService _movieSearchService;
    private readonly IChatCacheRepository _cacheRepository;
    private readonly ContextStrategyFactory _strategyFactory;
    private readonly ILogger<MovieConversationOrchestrator> _logger;

    public MovieConversationOrchestrator(
        IChatClient chatClient,
        IMovieSearchService movieSearchService,
        IChatCacheRepository cacheRepository,
        ContextStrategyFactory strategyFactory,
        ILogger<MovieConversationOrchestrator> logger)
    {
        _chatClient = chatClient;
        _movieSearchService = movieSearchService;
        _cacheRepository = cacheRepository;
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    public async Task<QueryResult> ProcessQuery(string userId, string query)
    {
        var startTime = DateTime.UtcNow;
        using var scope = _logger.BeginScope("ProcessQuery for user {UserId}", userId);
        _logger.LogInformation("Starting query processing: {Query}", query);

        try
        {
            var recentMessages = await _cacheRepository.GetRecentChatHistory(userId);
            
            if (!await IsMovieIntent(query, recentMessages))
            {
                _logger.LogInformation("Query not related to movies");
                return new QueryResult(
                    "I'm a movie assistant. Could you ask me something about movies?",
                    string.Empty,
                    DateTime.UtcNow - startTime
                );
            }

            var context = await BuildQueryContext(userId, query, recentMessages);
            var response = await _chatClient.GenerateResponseWithContextAsync(userId, query, context);

            await StoreInteraction(userId, query, response);

            return new QueryResult(response, context, DateTime.UtcNow - startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query");
            throw;
        }
    }

    private async Task<bool> IsMovieIntent(string query, List<ChatMessage> recentMessages)
    {
        var intentCheckStart = DateTime.UtcNow;
        _logger.LogInformation("Checking movie intent with {MessageCount} messages of context", recentMessages.Count);
        
        var isMovieIntent = await _chatClient.IsIntentForMovieDiscussionUsingAI(query, recentMessages);
        
        _logger.LogDuration("Intent check", intentCheckStart);
        return isMovieIntent;
    }

    private async Task<string> BuildQueryContext(string userId, string query, List<ChatMessage> recentMessages)
    {
        var startTime = DateTime.UtcNow;
        
        // Try direct movie lookup
        var movie = await FindRelevantMovie(query, recentMessages);
        if (movie != null)
        {
            var singleMovieStrategy = _strategyFactory.GetStrategy(ContextType.SingleMovie);
            return await singleMovieStrategy.BuildContext(movie, query);
        }

        // Try similar movies
        var similarMovies = await _movieSearchService.FindSimilarMoviesAsync(query);
        if (similarMovies.Any())
        {
            var similarMoviesStrategy = _strategyFactory.GetStrategy(ContextType.SimilarMovies);
            return await similarMoviesStrategy.BuildContext(new { Movies = similarMovies.Take(3).ToList() }, query);
        }

        // Fallback to conversation
        var conversationStrategy = _strategyFactory.GetStrategy(ContextType.Conversation);
        return await conversationStrategy.BuildContext(new { Messages = recentMessages }, query);
    }

    private async Task<MovieSummary?> FindRelevantMovie(string query, List<ChatMessage> recentMessages)
    {
        _logger.LogInformation("Starting FindRelevantMovie with query: {Query}", query);
        
        // Create a list of message contents, including the current query
        var allMessages = recentMessages.Select(m => m.Content).ToList();
        allMessages.Add(query);
        _logger.LogInformation("Processing {MessageCount} messages including current query", allMessages.Count);

        // Check if query is related to existing context from recent messages
        var existingContext = string.Join("\n", recentMessages.Select(m => m.Content));
        _logger.LogDebug("Built context from {MessageCount} recent messages. Context length: {ContextLength}", 
            recentMessages.Count, existingContext.Length);

        var isRelatedToContext = !string.IsNullOrEmpty(existingContext) && 
            await _chatClient.IsQueryRelatedToExistingContext(query, existingContext);
        _logger.LogInformation("Query is {RelatedStatus} to existing context", 
            isRelatedToContext ? "related" : "not related");

        // If related to context, use all messages to detect movie, otherwise just use current query
        _logger.LogInformation("Using {Strategy} to detect movie name", 
            isRelatedToContext ? "full conversation context" : "current query only");
            
        var movieName = isRelatedToContext 
            ? await _chatClient.DetectMovieNameUsingAI(allMessages)
            : await _chatClient.DetectMovieNameUsingAI(query);

        if (string.IsNullOrEmpty(movieName))
        {
            _logger.LogInformation("No movie name detected");
            return null;
        }

        _logger.LogInformation("Detected movie name: {MovieName}", movieName);
        var movie = await _movieSearchService.LookupMovieByNameAsync(movieName);
        _logger.LogInformation("Movie lookup {Result}", movie?.Plot != null ? "successful" : "failed");
        
        return movie?.Plot != null ? movie : null;
    }

    private async Task StoreInteraction(string userId, string query, string response)
    {
        await _cacheRepository.StoreMessageInHistory(userId, "user", query);
        await _cacheRepository.StoreMessageInHistory(userId, "assistant", response);
    }
} 