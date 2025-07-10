using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RobotoAgentAPI.Agents;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobotoAgentAPI.Controllers;

public class ChatRequest
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
}

public class ChatResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}

public class MovieResponse
{
    public List<Movie> SimilarMovies { get; set; } = new();
    public string IntelligentResponse { get; set; } = string.Empty;
}

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public float SimilarityScore { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly Kernel _kernel;

    public ChatController(Kernel kernel)
    {
        _kernel = kernel;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet("debug/plugins")]
    public IActionResult DebugPlugins()
    {
        var plugins = _kernel.Plugins.Select(p => new
        {
            Name = p.Name,
            Functions = p.Select(f => new
            {
                Name = f.Name,
                Description = f.Description
            }).ToArray()
        }).ToArray();

        return Ok(new { plugins });
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            // Log the incoming message for debugging
            Console.WriteLine($"Received chat request - UserId: '{request?.UserId}', Query: '{request?.Query}', Query Length: {request?.Query?.Length ?? 0} bytes");
            
            // Validate request
            if (request == null || string.IsNullOrWhiteSpace(request.Query))
            {
                Console.WriteLine("Validation failed: Query is null, empty, or whitespace");
                return BadRequest(new ErrorResponse
                {
                    Error = "BadRequest",
                    Message = "Query cannot be empty",
                    Details = "Please provide a valid query"
                });
            }

            // Rate limiting headers
            Response.Headers["X-RateLimit-Limit"] = "100";
            Response.Headers["X-RateLimit-Remaining"] = "99";
            Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

            // Get the ChatAgent plugin
            var chatAgent = _kernel.Plugins["ChatAgent"];
            
            // Call ChatAgent's ProcessMessage function
            var chatArguments = new KernelArguments
            {
                ["message"] = request.Query,
                ["userId"] = request.UserId ?? "default"
            };
            
            Console.WriteLine($"Calling ChatAgent.ProcessMessage with query: '{request.Query}' for user: '{request.UserId}'");
            var chatResult = await chatAgent["ProcessMessage"].InvokeAsync(_kernel, chatArguments);
            Console.WriteLine($"ChatAgent returned: {chatResult}");
            
            // Parse the result and create a text response
            var movieResponse = JsonSerializer.Deserialize<MovieResponse>(chatResult.ToString() ?? "{}");
            
            string textResponse;
            if (movieResponse?.SimilarMovies?.Any() == true)
            {
                // Use the intelligent response from ChatAgent if available
                if (!string.IsNullOrEmpty(movieResponse.IntelligentResponse))
                {
                    textResponse = movieResponse.IntelligentResponse;
                }
                else
                {
                    // Fallback to simple movie list if no intelligent response
                    var movieTitles = movieResponse.SimilarMovies.Select(m => m.Title).ToList();
                    if (movieTitles.Count == 1)
                    {
                        textResponse = $"I found this movie for you: {movieTitles[0]}.";
                    }
                    else
                    {
                        var lastMovie = movieTitles.Last();
                        var otherMovies = string.Join(", ", movieTitles.Take(movieTitles.Count - 1));
                        textResponse = $"Here are some movie recommendations: {otherMovies}, and {lastMovie}.";
                    }
                }
            }
            else
            {
                textResponse = "I couldn't find any movies matching your request. Please try a different search or ask about specific movies.";
            }

            return Ok(new ChatResponse { Response = textResponse });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Chat endpoint: {ex}");
            return StatusCode(500, new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An error occurred while processing your request",
                Details = ex.Message
            });
        }
    }

    [HttpPost("agentic")]
    public async Task<IActionResult> ChatAgentic([FromBody] ChatRequest request)
    {
        try
        {
            Console.WriteLine($"[AGENTIC] Processing request - UserId: '{request?.UserId}', Query: '{request?.Query}'");
            
            // Validate request
            if (request == null || string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "BadRequest",
                    Message = "Query cannot be empty",
                    Details = "Please provide a valid query"
                });
            }

            // Rate limiting headers
            Response.Headers["X-RateLimit-Limit"] = "100";
            Response.Headers["X-RateLimit-Remaining"] = "99";
            Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

            // Use the ChatAgent plugin with agentic processing
            var chatAgent = _kernel.Plugins["ChatAgent"];
            
            if (chatAgent == null)
            {
                Console.WriteLine($"[AGENTIC] ERROR: ChatAgent plugin not found!");
                return StatusCode(500, new ErrorResponse
                {
                    Error = "PluginNotFound",
                    Message = "ChatAgent plugin not found",
                    Details = "The ChatAgent plugin is not registered"
                });
            }
            
            var chatArguments = new KernelArguments
            {
                ["message"] = request.Query,
                ["userId"] = request.UserId ?? "default"
            };
            
            var chatResult = await chatAgent["ProcessMessageAgentic"].InvokeAsync(_kernel, chatArguments);
            
            if (chatResult?.ToString() != null)
            {
                var resultString = chatResult.ToString();
                
                // Parse the result to extract the intelligent response
                try
                {
                    var movieResponse = JsonSerializer.Deserialize<MovieResponse>(resultString);
                    var textResponse = movieResponse?.IntelligentResponse ?? "I'm sorry, I couldn't process your request.";
                    
                    return Ok(new ChatResponse { Response = textResponse });
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"[AGENTIC] JSON parsing failed: {jsonEx.Message}");
                    return Ok(new ChatResponse { Response = $"Raw response: {resultString}" });
                }
            }
            else
            {
                Console.WriteLine($"[AGENTIC] Result is null or empty");
                return Ok(new ChatResponse { Response = "No response from agentic processing" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AGENTIC] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            return StatusCode(500, new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An error occurred while processing your agentic request",
                Details = ex.Message
            });
        }
    }

    [HttpGet("debug/test-search")]
    public async Task<IActionResult> TestSearchAgent([FromQuery] string query = "action movies")
    {
        try
        {
            // Get the SearchAgent plugin directly
            var searchAgent = _kernel.Plugins["SearchAgent"];
            
            // Call SearchAgent's HybridSearch function directly
            var searchArguments = new KernelArguments
            {
                ["query"] = query
            };
            
            Console.WriteLine($"Direct SearchAgent test with query: '{query}'");
            var searchResult = await searchAgent["HybridSearch"].InvokeAsync(_kernel, searchArguments);
            Console.WriteLine($"Direct SearchAgent result: {searchResult}");
            
            return Ok(new { 
                query = query,
                result = searchResult.ToString(),
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TestSearchAgent: {ex}");
            return StatusCode(500, new ErrorResponse
            {
                Error = "TestError",
                Message = "Failed to test SearchAgent",
                Details = ex.Message
            });
        }
    }

    [HttpGet("debug/test-functions")]
    public async Task<IActionResult> TestFunctions([FromQuery] string message = "test message")
    {
        try
        {
            Console.WriteLine($"[DEBUG] Starting function test with message: '{message}'");
            
            var result = new
            {
                timestamp = DateTime.UtcNow,
                message = message,
                kernelPluginCount = _kernel.Plugins.Count,
                plugins = _kernel.Plugins.Select(p => new
                {
                    name = p.Name,
                    functionCount = p.Count(),
                    functions = p.Select(f => f.Name).ToArray()
                }).ToArray()
            };
            
            Console.WriteLine($"[DEBUG] Kernel has {_kernel.Plugins.Count} plugins");
            
            // Test ChatAgent plugin specifically
            var chatAgent = _kernel.Plugins["ChatAgent"];
            if (chatAgent != null)
            {
                Console.WriteLine($"[DEBUG] ChatAgent plugin found with {chatAgent.Count()} functions");
                
                // Test calling ProcessMessageAgentic directly
                var testArgs = new KernelArguments
                {
                    ["message"] = message,
                    ["userId"] = "debug-test"
                };
                
                Console.WriteLine($"[DEBUG] Attempting to call ProcessMessageAgentic...");
                var agenticResult = await chatAgent["ProcessMessageAgentic"].InvokeAsync(_kernel, testArgs);
                Console.WriteLine($"[DEBUG] ProcessMessageAgentic returned: '{agenticResult?.ToString()}'");
                
                return Ok(new
                {
                    kernelInfo = result,
                    agenticTest = new
                    {
                        success = true,
                        result = agenticResult?.ToString(),
                        resultLength = agenticResult?.ToString()?.Length ?? 0
                    }
                });
            }
            else
            {
                Console.WriteLine($"[DEBUG] ChatAgent plugin NOT found");
                return Ok(new
                {
                    kernelInfo = result,
                    agenticTest = new
                    {
                        success = false,
                        error = "ChatAgent plugin not found"
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Exception: {ex}");
            return Ok(new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
} 