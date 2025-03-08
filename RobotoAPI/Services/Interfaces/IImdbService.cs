public interface IImdbService
{
    Task<MovieDetails> GetMovieDetailsAsync(string imdbNumber);
} 