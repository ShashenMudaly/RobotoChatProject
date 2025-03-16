namespace ChatApp.Services.Interfaces;

public interface ITextSummarizationService
{
    Task<string> SummarizePlot(string plot);
    Task<string> GetAbstractiveSummary(string plot);
} 