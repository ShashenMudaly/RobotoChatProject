using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Extensions;
using ChatApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatApp.Services;

public class PlotProcessor
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<PlotProcessor> _logger;

    public PlotProcessor(IChatClient chatClient, ILogger<PlotProcessor> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<string> ProcessPlot(string plot, string query, string movieName)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Processing plot for movie: {MovieName}. Length: {Length}", movieName, plot.Length);

        var relevantChunks = await GetRelevantPlotChunks(plot, query);
        var result = relevantChunks.Any()
            ? FormatRelevantChunks(movieName, relevantChunks)
            : FormatFullPlot(movieName, plot);

        _logger.LogDuration("Plot processing", startTime);
        return result;
    }

    private async Task<List<string>> GetRelevantPlotChunks(string plot, string query)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting plot chunk analysis for query: {Query}", query);

        var allChunks = MoviePlotChunker.ChunkPlot(plot);
        var relevantChunks = new List<string>();

        _logger.LogInformation("Total plot chunks created: {Count}", allChunks.Count);

        foreach (var chunk in allChunks)
        {
            if (await _chatClient.CanAnswerFromContext(query, chunk))
            {
                relevantChunks.Add(chunk);
            }
        }

        _logger.LogInformation(
            "Found {RelevantCount} relevant chunks out of {TotalCount} total chunks",
            relevantChunks.Count, allChunks.Count);

        return relevantChunks;
    }

    private string FormatRelevantChunks(string movieName, List<string> chunks) =>
        $"{movieName} movie plot sections:\n{string.Join("\n", chunks)}";

    private string FormatFullPlot(string movieName, string plot) =>
        $"{movieName} movie plot: {plot}";
} 