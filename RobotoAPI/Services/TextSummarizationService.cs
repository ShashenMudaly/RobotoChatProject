using Azure.AI.TextAnalytics;
using Azure;
using Azure.AI.TextAnalytics.Models;
using ChatApp.Services.Interfaces;

namespace ChatApp.Services;

public class TextSummarizationService : ITextSummarizationService
{
    private readonly TextAnalyticsClient _client;
    private readonly ILogger<TextSummarizationService> _logger;

    public TextSummarizationService(TextAnalyticsClient client, ILogger<TextSummarizationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> SummarizePlot(string plot)
    {
        try
        {
            _logger.LogInformation("Summarizing plot of length: {Length}", plot.Length);
            
            // If plot is short enough, summarize directly
            if (plot.Length <= 2000)
            {
                return await SummarizePart(plot);
            }

            // Break into 2000 character chunks and summarize each
            var chunks = ChunkText(plot, 2000);
            _logger.LogInformation("Split plot into {Count} chunks", chunks.Count);

            var summaries = new List<string>();
            foreach (var chunk in chunks)
            {
                var summary = await SummarizePart(chunk);
                if (!string.IsNullOrEmpty(summary))
                {
                    summaries.Add(summary);
                }
            }

            var result = string.Join(" ", summaries);
            _logger.LogInformation("Combined summary length: {Length}", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in plot summarization");
            return plot;
        }
    }

    private async Task<string> SummarizePart(string text)
    {
        var operation = await _client.StartAnalyzeActionsAsync(new[] { text }, 
            new TextAnalyticsActions { 
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { 
                    new ExtractiveSummarizeAction() {
                        MaxSentenceCount = 5
                    }
                }
            });

        await operation.WaitForCompletionAsync();

        await foreach (var result in operation.Value)
        {
            var summary = result.ExtractiveSummarizeResults
                ?.SelectMany(r => r.DocumentsResults)
                ?.SelectMany(d => d.Sentences)
                ?.Select(s => s.Text)
                ?.FirstOrDefault();
            
            if (!string.IsNullOrEmpty(summary))
                return summary;
        }
        return text;
    }

    private List<string> ChunkText(string text, int chunkSize)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            var length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }
        return chunks;
    }
} 