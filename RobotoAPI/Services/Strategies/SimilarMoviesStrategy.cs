using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Services.Builders;
using ChatApp.Services.Interfaces;
using ChatApp.Services.Models;
using Microsoft.Extensions.Logging;

namespace ChatApp.Services.Strategies;

public class SimilarMoviesStrategy : IContextStrategy
{
    private readonly PlotProcessor _plotProcessor;
    private readonly ITextSummarizationService _summarizationService;
    private readonly ILogger<SimilarMoviesStrategy> _logger;

    public SimilarMoviesStrategy(
        PlotProcessor plotProcessor,
        ITextSummarizationService summarizationService,
        ILogger<SimilarMoviesStrategy> logger)
    {
        _plotProcessor = plotProcessor;
        _summarizationService = summarizationService;
        _logger = logger;
    }

    public async Task<string> BuildContext(object contextData, string query)
    {
        if (contextData is MovieSummary movie)
        {
            return await BuildContext(movie, query);
        }
        
        if (contextData is { } data && data.GetType().GetProperty("Movies")?.GetValue(data) is List<MovieSummary> movies)
        {
            return await BuildContextForMultiple(movies, query);
        }

        return string.Empty;
    }

    private async Task<string> BuildContext(MovieSummary movie, string query)
    {
        _logger.LogInformation("Building similar movies context starting with: {MovieName}", movie.Name);
        
        var context = new MovieContextBuilder()
            .AddTitle(movie.Name)
            .AddPlot(await _plotProcessor.ProcessPlot(movie.Plot, query, movie.Name))
            .Build();

        _logger.LogInformation("Similar movies context built, length: {Length}", context.Length);
        return context;
    }

    public async Task<string> BuildContextForMultiple(List<MovieSummary> movies, string query)
    {
        if (!movies.Any()) return string.Empty;

        var mainMovie = movies.First();
        var context = new MovieContextBuilder()
            .AddTitle(mainMovie.Name)
            .AddPlot(await _plotProcessor.ProcessPlot(mainMovie.Plot, query, mainMovie.Name));

        if (movies.Count > 1)
        {
            context.AddLine("\nOther similar movies:");
            foreach (var movie in movies.Skip(1))
            {
                var summarizedPlot = await _summarizationService.SummarizePlot(movie.Plot);
                context.AddLine($"- {movie.Name}: {summarizedPlot}");
            }
        }

        var result = context.Build();
        _logger.LogInformation("Similar movies context built with {Count} movies, length: {Length}", 
            movies.Count, result.Length);
        return result;
    }
} 