using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using ChatApp.Services.Interfaces;
using Microsoft.Extensions.Options;
using ChatApp.Options;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatApp.Services.Models;

namespace ChatApp.Services;

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

    public Task<bool> IsIntentForMovieDiscussionUsingAI(string userInput)
    {
        return IsIntentForMovieDiscussionUsingAI(userInput, null);
    }

    public async Task<bool> IsIntentForMovieDiscussionUsingAI(string userInput, List<ChatMessage>? conversationHistory)
    {
        var prompt = BuildMovieIntentPrompt(userInput, conversationHistory);
        
        try
        {
            var response = await _client.GetChatCompletionsAsync(
                new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages = {
                        new ChatRequestSystemMessage(
                            "You are a helpful assistant that analyzes conversations about movies. " +
                            "Analyze if the provided message is about movies or cinema-related topics. " +
                            "Consider both the current message and any conversation history provided. " +
                            "\n\nRespond with 'true' if the message:" +
                            "\n- Discusses movies, cinema, actors, directors, or film topics" +
                            "\n- Continues a previous movie-related conversation" +
                            "\n\nRespond with 'false' if the message:" +
                            "\n- Is not related to movies or cinema" +
                            "\n- Changes to a non-movie topic" +
                            "\n\nProvide ONLY 'true' or 'false' as your response."
                        ),
                        new ChatRequestUserMessage(prompt)
                    },
                    Temperature = 0,
                    MaxTokens = 10
                });

            var result = bool.Parse(response.Value.Choices[0].Message.Content.Trim());
            _logger.LogInformation(
                "Movie intent analysis - Query: {Query}, Has Context: {HasContext}, Result: {Result}", 
                userInput, 
                conversationHistory?.Any() ?? false, 
                result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing movie intent for query: {Query}", userInput);
            return false;
        }
    }

    private string BuildMovieIntentPrompt(string userInput, List<ChatMessage>? conversationHistory)
    {
        var promptBuilder = new StringBuilder();

        if (conversationHistory?.Any() == true)
        {
            promptBuilder.AppendLine("Previous conversation:");
            foreach (var message in conversationHistory.OrderBy(m => m.Timestamp))
            {
                promptBuilder.AppendLine($"{message.Role}: {message.Content}");
            }
            promptBuilder.AppendLine("\nCurrent query:");
        }

        promptBuilder.Append(userInput);
        return promptBuilder.ToString();
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
                            "You are a conversation continuity analyzer for movie discussions. Your task is to determine if the user's query is continuing the discussion about the same movie(s) from the previous context. " +
                            "Return 'true' ONLY if the query is asking about or referring to the same movie(s) being discussed in the context. " +
                            "Return 'false' if: \n" +
                            "1. The query is about a different movie than the one(s) in context\n" +
                            "2. The query starts a new movie-related topic\n" +
                            "3. The query is not about movies at all\n\n" +
                            "Respond ONLY with 'true' or 'false'. No other text."),
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

    public async Task<bool> CanAnswerFromContext(string query, string context)
    {
        try
        {
            _logger.LogInformation("Checking if query can be answered from provided context");
            var response = await _client.GetChatCompletionsAsync(
                new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages = {
                        new ChatRequestSystemMessage(
                            "You are a context analyzer. Your ONLY task is to determine if the user's query can be fully answered using ONLY the information in the provided context. " +
                            "You must NOT use any knowledge from your training data. " +
                            "Respond ONLY with 'true' if the context contains sufficient information to answer the query, or 'false' if external knowledge would be needed. " +
                            "Do not provide explanations or additional text. " +
                            "Context: " + context),
                        new ChatRequestUserMessage(query)
                    },
                    Temperature = 0f,
                    MaxTokens = 10
                });

            var responseText = response.Value.Choices[0].Message.Content.Trim().ToLower();
            _logger.LogInformation("Can answer from context: {CanAnswer}", responseText);
            return responseText == "true";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if query can be answered from context");
            return false;
        }
    }

    public async Task<string> DetectMovieNameUsingAI(List<string> messages)
    {
        var prompt = @"Given the following conversation messages, identify and extract the name of the most recently discussed movie. 
                Focus only on actual movie titles, not general movie discussions.
                If multiple movies are mentioned, return only the most recently mentioned one.
                If no specific movie is mentioned, return an empty string.

                Messages (from oldest to newest):
                ";
        foreach (var message in messages)
        {
            prompt += $"- {message}\n";
        }

        prompt += "\nMost recent movie title (return empty string if none found):";

        try
        {
            var response = await _client.GetChatCompletionsAsync(
                new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages = {
                        new ChatRequestSystemMessage("You are a movie detection assistant. Extract only the most recent movie title mentioned. Return empty string if no movie is found."),
                        new ChatRequestUserMessage(prompt)
                    },
                    Temperature = 0,
                    MaxTokens = 100
                });

            var movieName = response.Value.Choices[0].Message.Content.Trim();
            _logger.LogInformation("Detected movie name from messages: {MovieName}", movieName);
            return movieName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting movie name from messages");
            return string.Empty;
        }
    }
} 