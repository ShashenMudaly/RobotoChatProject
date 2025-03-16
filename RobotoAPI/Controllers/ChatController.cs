using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using ChatApp.Services.Interfaces;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMovieConversationOrchestrator _orchestrator;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IMovieConversationOrchestrator orchestrator,
            ILogger<ChatController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessChat([FromBody] ChatRequest request)
        {
            try
            {
                var (response, context, duration) = await _orchestrator.ProcessQuery(request.UserId, request.Query);
                return Ok(new { response, context });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private string BuildContextFromMovies(List<MovieSummary> movies)
        {
            var context = new StringBuilder();
            foreach (var movie in movies)
            {
                context.AppendLine($"Title: {movie.Name}");
                context.AppendLine($"Plot: {movie.Plot}");
                context.AppendLine();
            }
            return context.ToString();
        }

        private string GenerateChitChatContext()
        {
            var responses = new[]
            {
                "Let's talk about movies! What genres do you enjoy?",
                "I'd love to discuss cinema with you. What's the last great film you watched?",
                "Movies are fascinating! What aspects of film interest you the most?"
            };
            return responses[Random.Shared.Next(responses.Length)];
        }

        private string GenerateFallbackContext()
        {
            var responses = new[]
            {
                "I'm not quite sure what you're looking for. Could you mention a specific movie or describe what kind of film you're interested in?",
                "I'd love to help, but I need more details. Are you looking for a particular movie or genre?",
                "Could you provide more specific information about what you're looking for?"
            };
            return responses[Random.Shared.Next(responses.Length)];
        }
    }
} 