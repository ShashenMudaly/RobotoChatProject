public interface IMovieSearchService
{
    Task<MovieSummary?> LookupMovieByNameAsync(string movieName);
    Task<List<MovieSummary>> FindSimilarMoviesAsync(string query);
} 