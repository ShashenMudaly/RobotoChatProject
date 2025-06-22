using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;
using System.Collections.Concurrent;
using RobotoAgentAPI.Controllers;

namespace RobotoAgentAPI.Agents;

public class ChatAgentPlugin
{
    private readonly ChatProcessor _chatProcessor;
    private readonly ConversationManager _conversationManager;

    public ChatAgentPlugin(IChatCompletionService chatService)
    {
        _conversationManager = new ConversationManager();
        _chatProcessor = new ChatProcessor(chatService, _conversationManager);
    }

    [KernelFunction]
    [Description("Process user message with conversation history, validate if it's movie-related, and coordinate with SearchAgent")]
    public async Task<string> ProcessMessageAsync(
        Kernel kernel,
        [Description("The user's message")] string message,
        [Description("Unique identifier for the user/session")] string userId = "default")
    {
        return await _chatProcessor.ProcessMessageAsync(kernel, message, userId);
    }

    [KernelFunction]
    [Description("Clear conversation history for a specific user")]
    public string ClearConversationHistory(
        [Description("Unique identifier for the user/session")] string userId = "default")
    {
        return _conversationManager.ClearConversationHistory(userId);
    }

    [KernelFunction]
    [Description("Get conversation summary for a specific user")]
    public async Task<string> GetConversationSummary(
        [Description("Unique identifier for the user/session")] string userId = "default")
    {
        return await _conversationManager.GetConversationSummaryAsync(userId);
    }
}

