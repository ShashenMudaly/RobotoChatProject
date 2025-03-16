using System.Text;

namespace ChatApp.Services.Builders;

public class MovieContextBuilder
{
    private readonly StringBuilder _builder = new();

    public MovieContextBuilder AddTitle(string movieName)
    {
        _builder.AppendLine("Movie Information:");
        _builder.AppendLine($"Title: {movieName}");
        return this;
    }

    public MovieContextBuilder AddPlot(string processedPlot)
    {
        _builder.AppendLine(processedPlot);
        return this;
    }

    public MovieContextBuilder AddLine(string line)
    {
        _builder.AppendLine(line);
        return this;
    }

    public string Build() => _builder.ToString();
} 