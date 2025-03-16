using Azure.AI.TextAnalytics;
using Azure;
using Azure.AI.TextAnalytics.Models;
using ChatApp.Services.Interfaces;

namespace ChatApp.Services;

public class TextSummarizationService : ITextSummarizationService
{
    private readonly TextAnalyticsClient _client;
    private readonly ILogger<TextSummarizationService> _logger;
    private const int MAX_SUMMARY_SENTENCES = 15;

    public TextSummarizationService(TextAnalyticsClient client, ILogger<TextSummarizationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> SummarizePlot(string plot)
    {
        try
        {
            _logger.LogInformation("Starting extractive plot summarization. Original length: {Length}", plot.Length);
            
            if (string.IsNullOrEmpty(plot))
            {
                _logger.LogWarning("Empty plot provided for summarization");
                return string.Empty;
            }

            var summary = await GetAbstractiveSummary(plot);
            _logger.LogInformation("Extractive summary created. Length: {Length}", summary.Length);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during extractive plot summarization");
            return plot;
        }
    }

    public async Task<string> GetAbstractiveSummary(string plot)
    {
        try
        {
            _logger.LogInformation("Starting abstractive plot summarization. Original length: {Length}", plot.Length);
            
            if (string.IsNullOrEmpty(plot))
            {
                _logger.LogWarning("Empty plot provided for abstractive summarization");
                return string.Empty;
            }

            try
            {
                var operation = await _client.StartAnalyzeActionsAsync(new[] { plot }, 
                    new TextAnalyticsActions { 
                        AbstractiveSummarizeActions = new List<AbstractiveSummarizeAction>() { 
                            new AbstractiveSummarizeAction(new AbstractiveSummarizeOptions() {
                                SentenceCount = 15
                            })
                        }
                    });

                await operation.WaitForCompletionAsync();

                await foreach (var result in operation.Value)
                {
                    if (result.AbstractiveSummarizeResults?.FirstOrDefault()?.DocumentsResults?.FirstOrDefault()?.Summaries != null)
                    {
                        var summaryParts = result.AbstractiveSummarizeResults
                            .First()
                            .DocumentsResults
                            .First()
                            .Summaries
                            .Select(s => s.Text);

                        if (summaryParts.Any())
                        {
                            var summary = string.Join(" ", summaryParts);
                            _logger.LogInformation("Abstractive summary created. Length: {Length}, Content: {Summary}", 
                                summary.Length, summary);
                            return summary;
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Abstractive summarization not supported or failed. Falling back to extractive summarization.");
            }
            
            // Fall back to extractive summarization
            _logger.LogInformation("Falling back to extractive summarization");
            return await GetExtractiveSummary(plot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during abstractive plot summarization");
            return plot;
        }
    }

    private async Task<string> GetExtractiveSummary(string text)
    {
        var operation = await _client.StartAnalyzeActionsAsync(new[] { text }, 
            new TextAnalyticsActions { 
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { 
                    new ExtractiveSummarizeAction() {
                        MaxSentenceCount = MAX_SUMMARY_SENTENCES
                    }
                }
            });

        await operation.WaitForCompletionAsync();

        await foreach (var result in operation.Value)
        {
            var summaryParts = result.ExtractiveSummarizeResults
                ?.SelectMany(r => r.DocumentsResults)
                ?.SelectMany(d => d.Sentences)
                ?.Select(s => s.Text);

            if (summaryParts != null && summaryParts.Any())
            {
                var summary = string.Join(" ", summaryParts);
                _logger.LogInformation("Extractive summary created. Length: {Length}, Content: {Summary}", 
                    summary.Length, summary);
                return summary;
            }
        }
        
        _logger.LogWarning("No summary could be generated, returning original text. Length: {Length}", text.Length);
        return text;
    }
} 