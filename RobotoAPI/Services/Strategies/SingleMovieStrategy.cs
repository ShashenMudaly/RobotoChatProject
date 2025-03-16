using System.Threading.Tasks;
using ChatApp.Services.Builders;
using ChatApp.Services.Interfaces;
using ChatApp.Services.Models;
using Microsoft.Extensions.Logging;

namespace ChatApp.Services.Strategies;

public class SingleMovieStrategy : IContextStrategy
{
    private readonly PlotProcessor _plotProcessor;
    private readonly ILogger<SingleMovieStrategy> _logger;

    public SingleMovieStrategy(PlotProcessor plotProcessor, ILogger<SingleMovieStrategy> logger)
    {
        _plotProcessor = plotProcessor;
        _logger = logger;
    }

    public async Task<string> BuildContext(object contextData, string query)
    {
        if (contextData is MovieSummary movie)
        {
            _logger.LogInformation("Building single movie context for: {MovieName}", movie.Name);
            
            var context = new MovieContextBuilder()
                .AddTitle(movie.Name)
                .AddPlot(await _plotProcessor.ProcessPlot(movie.Plot, query, movie.Name))
                .Build();

            _logger.LogInformation("Single movie context built, length: {Length}", context.Length);
            return context;
        }

        return string.Empty;
    }
} 