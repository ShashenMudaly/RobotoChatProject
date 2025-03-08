using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using ChatApp.Services.Interfaces;
using Microsoft.Extensions.Options;
using ChatApp.Options;

public class ChatClient : IChatClient
{
    private readonly Azure.AI.OpenAI.OpenAIClient _client;
    private readonly IChatCacheRepository _cacheRepository;
    private readonly ILogger<ChatClient> _logger;
    private readonly string _deploymentName;

    public ChatClient(
        Azure.AI.OpenAI.OpenAIClient client,
        IChatCacheRepository cacheRepository,
        IOptions<ChatClientOptions> options,
        ILogger<ChatClient> logger)
    {
        _client = client;
        _cacheRepository = cacheRepository;
        _logger = logger;
        _deploymentName = options.Value.DeploymentName;
        
        _logger.LogInformation("ChatClient initialized with deployment: {DeploymentName}", _deploymentName);
    }

    public async Task<string> DetectMovieNameUsingAI(string userInput)
    {
        _logger.LogInformation("Detecting movie name using deployment: {DeploymentName}", _deploymentName);
        try
        {
            var response = await _client.GetChatCompletionsAsync(
                new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages = {
                        new ChatRequestSystemMessage("Extract the movie name from the user's message. If no movie is mentioned, respond with an empty string."),
                        new ChatRequestUserMessage(userInput)
                    },
                    Temperature = 0f,
                    MaxTokens = 100
                });

            return response.Value.Choices[0].Message.Content.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting movie name with deployment {DeploymentName}", _deploymentName);
            throw;
        }
    }

    public async Task<bool> IsIntentForMovieDiscussionUsingAI(string userInput)
    {
        var response = await _client.GetChatCompletionsAsync(
            new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages = {
                    new ChatRequestSystemMessage("Determine if the user is trying to discuss movies. Respond with 'true' or 'false' only."),
                    new ChatRequestUserMessage(userInput)
                },
                Temperature = 0,
                MaxTokens = 10
            });

        return bool.Parse(response.Value.Choices[0].Message.Content.Trim());
    }

    public async Task<string> GenerateResponseWithContextAsync(string userId, string query, string context)
    {
        var response = await _client.GetChatCompletionsAsync(
            new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages = {
                    new ChatRequestSystemMessage(
                        "You are a movie assistant. ONLY use the provided context to answer questions. " +
                        "If the context doesn't contain enough information to answer, say so. " +
                        "Do not use any external knowledge or training data. " +
                        "Context: " + context),
                    new ChatRequestUserMessage(query)
                },
                Temperature = 0.7f,
                MaxTokens = 500
            });

        return response.Value.Choices[0].Message.Content.Trim();
    }

    public async Task<bool> IsQueryRelatedToExistingContext(string query, string context)
    {
        try
        {
            _logger.LogInformation("Checking if query is related to existing context");
            var response = await _client.GetChatCompletionsAsync(
                new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages = {
                        new ChatRequestSystemMessage(
                            "You are a context analyzer. Your ONLY task is to determine if the user's query relates to the subject matter of the provided context. " +
                            "You must ONLY respond with the word 'true' or 'false'. " +
                            "Do not provide explanations or additional text. " +
                            "Context: " + context),
                        new ChatRequestUserMessage(query)
                    },
                    Temperature = 0f,
                    MaxTokens = 10
                });

            var responseText = response.Value.Choices[0].Message.Content.Trim().ToLower();
            _logger.LogInformation("Raw response from AI: {Response}", responseText);
            return responseText == "true";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking query relation to context");
            return false;
        }
    }

    private async Task<string> GetFormattedChatHistory(string userId)
    {
        var messages = await _cacheRepository.GetRecentChatHistory(userId);
        var context = new StringBuilder();
        foreach (var message in messages)
        {
            context.AppendLine($"{message.Role}: {message.Content}");
        }
        return context.ToString();
    }
} 