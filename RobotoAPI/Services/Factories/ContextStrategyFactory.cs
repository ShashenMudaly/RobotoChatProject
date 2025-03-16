using ChatApp.Services.Interfaces;
using ChatApp.Services.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Services.Factories;

public class ContextStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ContextStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IContextStrategy GetStrategy(ContextType type)
    {
        return type switch
        {
            ContextType.SingleMovie => _serviceProvider.GetRequiredService<SingleMovieStrategy>(),
            ContextType.SimilarMovies => _serviceProvider.GetRequiredService<SimilarMoviesStrategy>(),
            ContextType.Conversation => _serviceProvider.GetRequiredService<ConversationStrategy>(),
            _ => throw new ArgumentException($"Unknown context type: {type}")
        };
    }
}

public enum ContextType
{
    SingleMovie,
    SimilarMovies,
    Conversation
} 