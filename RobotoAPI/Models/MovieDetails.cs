public class MovieDetails
{
    public string Title { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
    public string ImdbNumber { get; set; } = string.Empty;
    public double ImdbRating { get; set; }
    public int ReleaseYear { get; set; }
    public string Director { get; set; } = string.Empty;
    public List<string> Cast { get; set; } = new();
    public List<string> Genres { get; set; } = new();
} 