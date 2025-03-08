using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ChatApp.Services
{
    public class MovieSearchService : IMovieSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MovieSearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["MovieApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "MovieApi:BaseUrl is not configured");
        }

        public async Task<MovieSummary?> LookupMovieByNameAsync(string movieName)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Search/movie?name={Uri.EscapeDataString(movieName)}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MovieSummary>();
            }
            
            return null;
        }

        public async Task<List<MovieSummary>> FindSimilarMoviesAsync(string query)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Search/hybrid?query={Uri.EscapeDataString(query)}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<MovieSummary>>() ?? new List<MovieSummary>();
            }
            
            return new List<MovieSummary>();
        }
    }
} 