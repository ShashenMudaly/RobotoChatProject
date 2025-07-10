using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using RobotoAgentAPI.Controllers;

namespace RobotoAgentAPI.Agents;

public class SearchAgentPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _bonoApiKey;
    private readonly string _bonoEndpoint;
    private readonly IChatCompletionService _chatCompletion;

    public SearchAgentPlugin(string bonoApiKey, string bonoEndpoint, IChatCompletionService chatCompletion)
    {
        _bonoApiKey = bonoApiKey;
        _bonoEndpoint = bonoEndpoint;
        _chatCompletion = chatCompletion;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60); // Set reasonable timeout for Bono API calls
        if (!string.IsNullOrEmpty(_bonoApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bonoApiKey);
        }
        Console.WriteLine($"SearchAgentPlugin initialized with endpoint: {_bonoEndpoint}");
    }

    /// <summary>
    /// Answer movie-related queries using agentic approach where AI automatically decides which search functions to call
    /// This is NOT a KernelFunction to avoid recursive calling - it's the orchestrator method
    /// </summary>
    public async Task<string> AnswerMovieQueryAgentic(
        Kernel kernel,
        string query,
        string conversationHistory = "",
        string userId = "")
    {
        try
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Starting agentic search processing for query: '{query}'");
            
            // Create a chat history for the AI to understand the search context
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(@"You are an intelligent movie search assistant. You have access to several specialized functions to help users find movie information. Your goal is to provide the most helpful and accurate movie information by intelligently using the available functions.

Available functions:
- ExtractMovieNamesFromQueryAgentic: Use this to identify specific movie titles mentioned in queries
- SearchMoviesByName: Use this when you have specific movie titles to search for  
- HybridSearch: Use this for general searches, recommendations, or when no specific movie titles are found
- CheckRecentMovieContextAgentic: Use this to understand if the query relates to recently discussed movies
- GetMoviePlot: Use this to get detailed plot information for specific movies

Strategy:
1. For queries like 'Tell me about Star Wars movies' - first extract movie names, then search for them
2. For general queries like 'action movies' - use HybridSearch directly
3. Always provide helpful and informative responses about the movies you find

Be intelligent about which functions to call and in what order. You can call multiple functions if needed to provide the best answer.");

            if (!string.IsNullOrEmpty(conversationHistory))
            {
                chatHistory.AddUserMessage($"Conversation context: {conversationHistory}");
            }
            
            chatHistory.AddUserMessage($"Please help me find information about: {query}");
            
            // Configure auto-function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
            
            Console.WriteLine($"[SEARCH-AGENTIC] Calling AI with auto-function calling...");
            
            // Let the AI automatically decide which functions to call
            var response = await _chatCompletion.GetChatMessageContentAsync(
                chatHistory, 
                executionSettings, 
                kernel);
            
            Console.WriteLine($"[SEARCH-AGENTIC] AI response received");
            Console.WriteLine($"[SEARCH-AGENTIC] Response content: '{response.Content}'");
            
            // The AI should have called the appropriate functions and provided a response
            var aiResponse = response.Content ?? "I couldn't process your movie query.";
            
            // Try to format as MovieResponse if the response contains movie data
            try
            {
                // If the AI response looks like it contains movie information, format it properly
                if (aiResponse.Contains("movie") || aiResponse.Contains("film"))
                {
                    return JsonSerializer.Serialize(new MovieResponse 
                    { 
                        SimilarMovies = new List<Movie>(), // AI-driven approach focuses on text response
                        IntelligentResponse = aiResponse
                    });
                }
                else
                {
                    return JsonSerializer.Serialize(new MovieResponse 
                    { 
                        SimilarMovies = new List<Movie>(),
                        IntelligentResponse = aiResponse
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEARCH-AGENTIC] Error formatting response: {ex.Message}");
                return JsonSerializer.Serialize(new MovieResponse 
                { 
                    SimilarMovies = new List<Movie>(),
                    IntelligentResponse = aiResponse
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEARCH-AGENTIC] ERROR in agentic search: {ex}");
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Error = "SearchError",
                Message = "Failed to process movie query using agentic approach",
                Details = ex.Message
            });
        }
    }

    [KernelFunction]
    [Description("Extract specific movie titles mentioned in a user query")]
    public async Task<string> ExtractMovieNamesFromQueryAgentic(
        [Description("The user's query to analyze for movie titles")] string query)
    {
        try
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Extracting movie names from query: '{query}'");
            
            var systemPrompt = @"You are a movie name extraction assistant. Your task is to identify specific movie titles mentioned in user queries.

Guidelines:
- Extract only actual movie titles that are explicitly mentioned
- Do not extract general movie concepts, genres, or descriptions
- Return movie titles exactly as mentioned, maintaining proper capitalization
- If no specific movie titles are found, return an empty response
- Separate multiple movie titles with commas
- Only extract titles you are confident are actual movie names

Examples:
- ""Tell me about The Matrix"" → ""The Matrix""
- ""What happens in Inception?"" → ""Inception""
- ""Compare The Dark Knight and Batman Begins"" → ""The Dark Knight,Batman Begins""
- ""I want action movies"" → """"
- ""Movies like sci-fi"" → """"
- ""Best horror films"" → """"";

            var userPrompt = $@"Extract movie titles from this query: ""{query}""

Return only the movie titles separated by commas, or return empty if no specific movie titles are found.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);

            var response = await _chatCompletion.GetChatMessageContentAsync(chatHistory);
            var extractedText = response.Content?.Trim() ?? "";
            
            Console.WriteLine($"[SEARCH-AGENTIC] AI extracted movie names: '{extractedText}'");
            return extractedText;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Error extracting movie names: {ex}");
            return "";
        }
    }

    [KernelFunction]
    [Description("Search for specific movies by their exact titles")]
    public async Task<string> SearchMoviesByName(
        [Description("Comma-separated list of movie titles to search for")] string movieNames)
    {
        try
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Searching for specific movies: {movieNames}");
            
            if (string.IsNullOrWhiteSpace(movieNames))
            {
                return "No movie names provided to search for.";
            }
            
            var movieList = movieNames.Split(',').Select(name => name.Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
            var allMovies = new List<Movie>();
            
            foreach (var movieName in movieList.Take(3)) // Limit to prevent too many API calls
            {
                var encodedMovieName = Uri.EscapeDataString(movieName);
                var requestUrl = $"{_bonoEndpoint}/api/search/movie?name={encodedMovieName}";
                
                Console.WriteLine($"[SEARCH-AGENTIC] Making movie search request to: {requestUrl}");
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SEARCH-AGENTIC] Movie search failed for '{movieName}', status: {response.StatusCode}");
                    continue;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SEARCH-AGENTIC] Raw API response for '{movieName}': {content}");
                
                try
                {
                    var movieSearchResponse = JsonSerializer.Deserialize<BonoSingleMovieResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (movieSearchResponse != null && !string.IsNullOrEmpty(movieSearchResponse.Name))
                    {
                        var movie = new Movie
                        {
                            Id = movieSearchResponse.Id.ToString(),
                            Title = movieSearchResponse.Name ?? movieName,
                            Plot = movieSearchResponse.Plot ?? "",
                            SimilarityScore = 1.0f
                        };
                        
                        allMovies.Add(movie);
                        Console.WriteLine($"[SEARCH-AGENTIC] Found movie: {movie.Title}");
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"[SEARCH-AGENTIC] JSON parsing error for '{movieName}': {jsonEx.Message}");
                }
            }
            
            if (allMovies.Any())
            {
                var foundMovies = string.Join(", ", allMovies.Select(m => m.Title));
                return $"Found {allMovies.Count} movie(s): {foundMovies}. " + 
                       string.Join(" ", allMovies.Select(m => $"{m.Title}: {m.Plot?.Substring(0, Math.Min(150, m.Plot?.Length ?? 0))}..."));
            }
            else
            {
                return $"No movies found for the search terms: {movieNames}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Error searching movies by name: {ex}");
            return $"Error searching for movies: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Check if a query relates to recently discussed movies in the conversation")]
    public async Task<string> CheckRecentMovieContextAgentic(
        [Description("The user's current query")] string query,
        [Description("Recent conversation history")] string conversationHistory = "")
    {
        try
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Checking recent movie context for query: '{query}'");
            
            if (string.IsNullOrWhiteSpace(conversationHistory))
            {
                return "No recent conversation history available to check context.";
            }
            
            var systemPrompt = @"You are a conversation context analyzer. Your job is to determine if a current query relates to movies that were recently discussed in the conversation.

Analyze the conversation history and current query to determine:
1. If the query relates to a recently mentioned movie
2. Which specific movie it relates to
3. How confident you are about this connection

Respond in this format:
Related: [YES/NO]
Movie: [Movie Title if related, or NONE]
Confidence: [Low/Medium/High]
Explanation: [Brief explanation of why/why not]";

            var userPrompt = $@"Conversation History:
{conversationHistory}

Current Query: ""{query}""

Analyze if this query relates to any recently discussed movies.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);

            var response = await _chatCompletion.GetChatMessageContentAsync(chatHistory);
            var analysisResult = response.Content?.Trim() ?? "Unable to analyze context.";
            
            Console.WriteLine($"[SEARCH-AGENTIC] Context analysis result: {analysisResult}");
            return analysisResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Error checking recent movie context: {ex}");
            return $"Error analyzing conversation context: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get detailed plot information for a specific movie by ID")]
    public async Task<string> GetMoviePlot(
        [Description("The movie ID to get plot information for")] string movieId)
    {
        try
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Getting plot for movie ID: {movieId}");
            
            var requestUrl = $"{_bonoEndpoint}/api/search/movie/{movieId}";
            var response = await _httpClient.GetAsync(requestUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"Could not retrieve plot for movie ID {movieId}. Status: {response.StatusCode}";
            }
            
            var content = await response.Content.ReadAsStringAsync();
            
            try
            {
                var movieDetails = JsonSerializer.Deserialize<BonoMovieDetails>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (movieDetails != null && !string.IsNullOrEmpty(movieDetails.Plot))
                {
                    return $"Plot for {movieDetails.Title}: {movieDetails.Plot}";
                }
                else
                {
                    return $"Plot information not available for movie ID {movieId}";
                }
            }
            catch (JsonException)
            {
                return $"Error parsing movie details for ID {movieId}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEARCH-AGENTIC] Error getting movie plot: {ex}");
            return $"Error retrieving plot: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Answer movie-related queries by searching for movies and analyzing their plots using AI")]
    public async Task<string> AnswerMovieQuery(
        [Description("The user's movie-related query")] string query,
        [Description("Recent conversation history for context")] string conversationHistory = "",
        [Description("User identifier for context")] string userId = "")
    {
        try
        {
            Console.WriteLine($"SearchAgent.AnswerMovieQuery called with query: '{query}' for user: '{userId}'");
            
            // First, extract movie names from the query using AI
            var extractedMovies = await ExtractMovieNamesFromQuery(query);
            
            string searchResult;
            
            if (extractedMovies.Any())
            {
                Console.WriteLine($"Extracted movie names: {string.Join(", ", extractedMovies)}");
                // Search for specific movies using the movie endpoint
                searchResult = await SearchMoviesByNameAsync(extractedMovies, query);
            }
            else
            {
                Console.WriteLine("No specific movie names found, checking if query relates to recent movie context");
                
                // Check if the query relates to recently discussed movies
                var recentMovieContext = await CheckRecentMovieContext(query, conversationHistory);
                
                if (recentMovieContext.IsRelatedToRecentMovie && !string.IsNullOrEmpty(recentMovieContext.MovieTitle))
                {
                    Console.WriteLine($"Query relates to recent movie: '{recentMovieContext.MovieTitle}'");
                    // Answer using the recent movie's context
                    searchResult = await AnswerAboutSpecificMovie(query, recentMovieContext, conversationHistory);
                }
                else
                {
                    Console.WriteLine("Query not related to recent movies, falling back to hybrid search");
                    // Fallback to hybrid search
                    searchResult = await HybridSearchAsync(query);
                }
            }
            
            // Parse the search result
            var searchResponse = JsonSerializer.Deserialize<MovieResponse>(searchResult);
            
            if (searchResponse?.SimilarMovies == null || !searchResponse.SimilarMovies.Any())
            {
                return JsonSerializer.Serialize(new MovieResponse 
                { 
                    SimilarMovies = new List<Movie>(),
                    IntelligentResponse = "I couldn't find any movies related to your query. Could you try rephrasing or asking about a different movie?"
                });
            }

            // Use AI to generate an intelligent response based on the query and movie data
            var intelligentResponse = await GenerateAIResponse(query, searchResponse.SimilarMovies);
            
            return JsonSerializer.Serialize(new MovieResponse 
            { 
                SimilarMovies = searchResponse.SimilarMovies,
                IntelligentResponse = intelligentResponse
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchAgent.AnswerMovieQuery: {ex}");
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Error = "SearchError",
                Message = "Failed to process movie query",
                Details = ex.Message
            });
        }
    }

    private async Task<List<string>> ExtractMovieNamesFromQuery(string query)
    {
        try
        {
            Console.WriteLine($"Extracting movie names from query: '{query}'");
            
            var systemPrompt = @"You are a movie name extraction assistant. Your task is to identify specific movie titles mentioned in user queries.

Guidelines:
- Extract only actual movie titles that are explicitly mentioned
- Do not extract general movie concepts, genres, or descriptions
- Return movie titles exactly as mentioned, maintaining proper capitalization
- If no specific movie titles are found, return an empty response
- Separate multiple movie titles with commas
- Only extract titles you are confident are actual movie names

Examples:
- ""Tell me about The Matrix"" → ""The Matrix""
- ""What happens in Inception?"" → ""Inception""
- ""Compare The Dark Knight and Batman Begins"" → ""The Dark Knight,Batman Begins""
- ""I want action movies"" → """"
- ""Movies like sci-fi"" → """"
- ""Best horror films"" → """"";

            var userPrompt = $@"Extract movie titles from this query: ""{query}""

Return only the movie titles separated by commas, or return empty if no specific movie titles are found.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);

            var response = await _chatCompletion.GetChatMessageContentAsync(chatHistory);
            var extractedText = response.Content?.Trim() ?? "";
            
            Console.WriteLine($"AI extracted movie names: '{extractedText}'");
            
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return new List<string>();
            }
            
            var movieNames = extractedText
                .Split(',')
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();
            
            return movieNames;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting movie names: {ex}");
            return new List<string>();
        }
    }

    private async Task<string> SearchMoviesByNameAsync(List<string> movieNames, string originalQuery)
    {
        try
        {
            Console.WriteLine($"Searching for specific movies: {string.Join(", ", movieNames)}");
            
            var allMovies = new List<Movie>();
            
            foreach (var movieName in movieNames.Take(3)) // Limit to prevent too many API calls
            {
                var encodedMovieName = Uri.EscapeDataString(movieName);
                var requestUrl = $"{_bonoEndpoint}/api/search/movie?name={encodedMovieName}";
                
                Console.WriteLine($"Making movie search request to: {requestUrl}");
                
                var response = await _httpClient.GetAsync(requestUrl);
                Console.WriteLine($"Movie search API response status for '{movieName}': {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Movie search failed for '{movieName}', status: {response.StatusCode}");
                    continue;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var truncatedContent = content.Length > 100 ? content.Substring(0, 100) + "..." : content;
                Console.WriteLine($"Movie search response for '{movieName}': {truncatedContent}");
                
                try
                {
                    // Try to deserialize as array first (hybrid search format)
                    var movieSearchResponse = JsonSerializer.Deserialize<BonoSearchResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (movieSearchResponse != null && movieSearchResponse.Any())
                    {
                        // Convert BonoSearchResult to Movie objects
                        foreach (var result in movieSearchResponse.Take(2)) // Limit results per movie
                        {
                            var movie = new Movie
                            {
                                Id = result.Id.ToString(),
                                Title = result.Name,
                                Year = 0,
                                SimilarityScore = 1.0f,
                                PosterUrl = "",
                                Plot = result.Plot
                            };
                            allMovies.Add(movie);
                        }
                    }
                }
                catch (JsonException)
                {
                    // If array deserialization fails, try single object
                    try
                    {
                        var singleResult = JsonSerializer.Deserialize<BonoSearchResult>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (singleResult != null && !string.IsNullOrEmpty(singleResult.Name))
                        {
                            var movie = new Movie
                            {
                                Id = singleResult.Id.ToString(),
                                Title = singleResult.Name,
                                Year = 0,
                                SimilarityScore = 1.0f,
                                PosterUrl = "",
                                Plot = singleResult.Plot
                            };
                            allMovies.Add(movie);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize movie response for '{movieName}': {ex.Message}");
                    }
                }
            }
            
            // If no movies found by name search, fallback to hybrid search
            if (!allMovies.Any())
            {
                Console.WriteLine("No movies found by name search, falling back to hybrid search");
                return await HybridSearchAsync(originalQuery);
            }
            
            return JsonSerializer.Serialize(new MovieResponse { SimilarMovies = allMovies });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchMoviesByNameAsync: {ex}");
            // Fallback to hybrid search on error
            return await HybridSearchAsync(originalQuery);
        }
    }

    [KernelFunction]
    [Description("Search for movies using hybrid search")]
    public async Task<string> HybridSearchAsync(
        [Description("The search query")] string query)
    {
        try
        {
            Console.WriteLine($"SearchAgent.HybridSearchAsync called with query: '{query}'");
            
            var encodedQuery = Uri.EscapeDataString(query);
            var requestUrl = $"{_bonoEndpoint}/api/search/hybrid?query={encodedQuery}";
            
            Console.WriteLine($"Making request to: {requestUrl}");
            
            var response = await _httpClient.GetAsync(requestUrl);
            Console.WriteLine($"Bono API response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase}).");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var truncatedContent = content.Length > 100 ? content.Substring(0, 100) + "..." : content;
            Console.WriteLine($"Raw Bono API response: {truncatedContent}");
            
            var searchResponse = JsonSerializer.Deserialize<BonoSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (searchResponse == null || !searchResponse.Any())
            {
                Console.WriteLine("No results found from Bono API");
                return JsonSerializer.Serialize(new MovieResponse { SimilarMovies = new List<Movie>() });
            }
            
            Console.WriteLine($"Bono API returned {searchResponse.Count} results");
            
            // Convert BonoSearchResult to Movie objects and fetch plots
            var movies = new List<Movie>();
            foreach (var result in searchResponse.Take(5))
            {
                var movie = new Movie
                {
                    Id = result.Id.ToString(),
                    Title = result.Name,
                    Year = 0, // Assuming year is not available in the search result
                    SimilarityScore = 1.0f, // Assuming a default similarity score
                    PosterUrl = "", // Assuming poster URL is not available in the search result
                    Plot = result.Plot
                };
                movies.Add(movie);
            }
            
            return JsonSerializer.Serialize(new MovieResponse { SimilarMovies = movies });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchAgent.HybridSearchAsync: {ex}");
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Error = "SearchError",
                Message = "Failed to perform movie search",
                Details = ex.Message
            });
        }
    }

    private async Task<string> GetMoviePlotAsync(string movieId)
    {
        try
        {
            var requestUrl = $"{_bonoEndpoint}/api/search/movie?id={movieId}";
            var response = await _httpClient.GetAsync(requestUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return "";
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var movieDetails = JsonSerializer.Deserialize<BonoMovieDetails>(content);
            
            return movieDetails?.Plot ?? "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching plot for movie {movieId}: {ex}");
            return "";
        }
    }

    private async Task<string> GenerateAIResponse(string userQuery, IEnumerable<Movie> movies)
    {
        try
        {
            var movieList = movies.ToList();
            Console.WriteLine($"Generating AI response for query: '{userQuery}' with {movieList.Count} movies");
            
            // Build context with movie information
            var movieContext = BuildMovieContext(movieList);
            
            // Create a prompt for the AI to analyze the query and provide a response
            var systemPrompt = @"You are a movie expert assistant. Your role is to answer user questions about movies based on the provided movie data and plots. 

Guidelines:
- Provide specific, accurate answers based on the movie plots and information provided
- If the user asks about characters, cast, plot details, or specific scenes, extract that information from the plots
- For recommendation requests, suggest movies from the provided list with brief explanations
- If you cannot find specific information in the plots, acknowledge this and provide what information you can
- Keep responses conversational and helpful
- Focus on the most relevant movie(s) for the user's question
- If multiple movies are relevant, mention the most appropriate ones first";

            var userPrompt = $@"User Query: {userQuery}

Available Movies and Plots:
{movieContext}

Please provide a helpful response to the user's query based on the movie information above.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);

            var response = await _chatCompletion.GetChatMessageContentAsync(chatHistory);
            
            Console.WriteLine($"AI generated response: {response.Content}");
            return response.Content ?? "I'm sorry, I couldn't generate a response for your query.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating AI response: {ex}");
            return "I encountered an error while processing your query. Please try again.";
        }
    }

    private string BuildMovieContext(List<Movie> movies)
    {
        var context = new List<string>();
        
        foreach (var movie in movies.Take(3)) // Limit to top 3 movies to avoid token limits
        {
            var movieInfo = $"**{movie.Title}** ({movie.Year})";
            if (!string.IsNullOrEmpty(movie.Plot))
            {
                // Truncate very long plots to avoid token limits
                var plot = movie.Plot.Length > 1500 ? movie.Plot.Substring(0, 1500) + "..." : movie.Plot;
                movieInfo += $"\nPlot: {plot}";
            }
            else
            {
                movieInfo += "\nPlot: Not available";
            }
            
            context.Add(movieInfo);
        }
        
        return string.Join("\n\n---\n\n", context);
    }

    private async Task<RecentMovieContext> CheckRecentMovieContext(string query, string conversationHistory)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conversationHistory))
            {
                return new RecentMovieContext { IsRelatedToRecentMovie = false };
            }

            Console.WriteLine($"Checking if query relates to recent movie context");
            
            var contextPrompt = @"You are analyzing whether a user's query relates to a recently discussed movie in the conversation history.

Your task:
1. Identify the most recently discussed movie from the conversation history
2. Determine if the current query relates to that movie (characters, plot, scenes, etc.)
3. Respond in JSON format

Response format:
{
    ""isRelatedToRecentMovie"": true/false,
    ""movieTitle"": ""Movie Name"" (if related, otherwise empty),
    ""moviePlot"": ""Movie plot summary"" (if available from conversation, otherwise empty),
    ""confidence"": 0.0-1.0
}

Examples:
- If conversation discussed 'Fight Club' and query is 'was Tyler real' → related to Fight Club
- If conversation discussed 'Inception' and query is 'how many levels were there' → related to Inception  
- If conversation discussed 'The Matrix' and query is 'tell me about Star Wars' → not related
- If no recent movie discussion found → not related

Conversation History:
" + conversationHistory + @"

Current Query: """ + query + @"""

Analysis (JSON only):";

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(contextPrompt);

            var response = await _chatCompletion.GetChatMessageContentAsync(chatHistory);
            var jsonResponse = response.Content?.Trim() ?? "";
            
            Console.WriteLine($"Recent movie context analysis: {jsonResponse}");

            // Try to parse the JSON response
            try
            {
                var analysis = JsonSerializer.Deserialize<RecentMovieContextResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new RecentMovieContext
                {
                    IsRelatedToRecentMovie = analysis?.IsRelatedToRecentMovie ?? false,
                    MovieTitle = analysis?.MovieTitle ?? "",
                    MoviePlot = analysis?.MoviePlot ?? "",
                    Confidence = analysis?.Confidence ?? 0.0f
                };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse recent movie context JSON: {ex.Message}");
                return new RecentMovieContext { IsRelatedToRecentMovie = false };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking recent movie context: {ex}");
            return new RecentMovieContext { IsRelatedToRecentMovie = false };
        }
    }

    private async Task<string> AnswerAboutSpecificMovie(string query, RecentMovieContext movieContext, string conversationHistory)
    {
        try
        {
            Console.WriteLine($"Answering question about specific movie: '{movieContext.MovieTitle}'");
            
            // If we have plot information from conversation, use it directly
            if (!string.IsNullOrEmpty(movieContext.MoviePlot))
            {
                return await GenerateSpecificMovieResponse(query, movieContext.MovieTitle, movieContext.MoviePlot, conversationHistory);
            }
            
            // Otherwise, try to search for the movie to get plot information
            var movieSearch = await SearchMoviesByNameAsync(new List<string> { movieContext.MovieTitle }, query);
            var searchResponse = JsonSerializer.Deserialize<MovieResponse>(movieSearch);
            
            if (searchResponse?.SimilarMovies?.Any() == true)
            {
                var movieWithPlot = searchResponse.SimilarMovies.FirstOrDefault(m => !string.IsNullOrEmpty(m.Plot));
                if (movieWithPlot != null)
                {
                    return await GenerateSpecificMovieResponse(query, movieWithPlot.Title, movieWithPlot.Plot, conversationHistory);
                }
            }
            
            // Fallback: Generate response with limited context
            return await GenerateSpecificMovieResponse(query, movieContext.MovieTitle, "", conversationHistory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error answering about specific movie: {ex}");
            // Fallback to hybrid search
            return await HybridSearchAsync(query);
        }
    }

    private async Task<string> GenerateSpecificMovieResponse(string query, string movieTitle, string moviePlot, string conversationHistory)
    {
        try
        {
            var responsePrompt = @"You are a movie expert assistant answering a specific question about a movie. Use the provided movie information and conversation context to give a detailed, accurate answer.

Guidelines:
- Answer the specific question asked
- Use information from the movie plot if available
- Reference the conversation context naturally
- If you cannot answer from the available information, say so honestly
- Keep the response focused and informative
- If no plot is provided, work with what information you have from the conversation

Movie: " + movieTitle + @"
" + (string.IsNullOrEmpty(moviePlot) ? "" : $"Plot: {moviePlot}") + @"

Recent Conversation Context:
" + conversationHistory + @"

User Question: """ + query + @"""

Your detailed answer:";

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(responsePrompt);

            var response = await _chatCompletion.GetChatMessageContentAsync(chatHistory);
            var movieResponse = response.Content?.Trim() ?? "I couldn't generate a proper response about this movie.";
            
            Console.WriteLine($"Generated specific movie response for '{movieTitle}'");
            
            // Return in the expected format
            var movies = new List<Movie>();
            if (!string.IsNullOrEmpty(moviePlot))
            {
                movies.Add(new Movie
                {
                    Id = "context",
                    Title = movieTitle,
                    Year = 0,
                    SimilarityScore = 1.0f,
                    PosterUrl = "",
                    Plot = moviePlot
                });
            }
            
            return JsonSerializer.Serialize(new MovieResponse
            {
                SimilarMovies = movies,
                IntelligentResponse = movieResponse
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating specific movie response: {ex}");
            return JsonSerializer.Serialize(new MovieResponse
            {
                SimilarMovies = new List<Movie>(),
                IntelligentResponse = "I encountered an error while answering your question about this movie."
            });
        }
    }

}

public class RecentMovieContext
{
    public bool IsRelatedToRecentMovie { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePlot { get; set; } = string.Empty;
    public float Confidence { get; set; }
}

public class RecentMovieContextResponse
{
    [JsonPropertyName("isRelatedToRecentMovie")]
    public bool IsRelatedToRecentMovie { get; set; }
    
    [JsonPropertyName("movieTitle")]
    public string MovieTitle { get; set; } = string.Empty;
    
    [JsonPropertyName("moviePlot")]
    public string MoviePlot { get; set; } = string.Empty;
    
    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }
}

public class BonoSearchResponse : List<BonoSearchResult>
{
    // This class now inherits from List to match the array response format
}

public class BonoSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("plot")]
    public string Plot { get; set; } = string.Empty;
}

public class BonoMovieDetails
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
}

public class BonoMovie
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
}

public class BonoSingleMovieResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("plot")]
    public string Plot { get; set; } = string.Empty;
} 