using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using RobotoAgentAPI.Controllers;

namespace RobotoAgentAPI.Agents;

public class ChatProcessor
{
    private readonly string _systemPrompt;
    private readonly IChatCompletionService _chatService;
    private readonly ConversationManager _conversationManager;
    private readonly SearchAgentPlugin _searchAgent;

    public ChatProcessor(IChatCompletionService chatService, ConversationManager conversationManager, SearchAgentPlugin searchAgent)
    {
        _systemPrompt = File.ReadAllText("Prompts/MovieAgentPrompt.txt");
        _chatService = chatService;
        _conversationManager = conversationManager;
        _searchAgent = searchAgent;
    }

    public async Task<string> ProcessMessageAsync(Kernel kernel, string message, string userId)
    {
        try
        {
            Console.WriteLine($"ChatProcessor processing message for user '{userId}': '{message}'");
            
            // Get or create conversation history for this user
            var chatHistory = _conversationManager.GetOrCreateUserConversation(userId, _systemPrompt);
            
            // Check if the query is movie-related using the language model (with context)
            var isMovieRelated = await IsMovieRelatedAsync(message, chatHistory);
            if (!isMovieRelated)
            {
                // Politely reject non-movie queries and steer toward movie conversations
                var rejectionResponse = await GenerateMovieSteeringResponseAsync(message, chatHistory);
                
                // Add to conversation history
                _conversationManager.AddToConversation(userId, message, rejectionResponse);
                
                return JsonSerializer.Serialize(new MovieResponse 
                { 
                    SimilarMovies = new List<Movie>(),
                    IntelligentResponse = rejectionResponse
                });
            }

            // Get the SearchAgent plugin
            var searchAgent = kernel.Plugins["SearchAgent"];
            
            // Enhanced search strategy: Check if query relates to recent movie context
            var searchArguments = new KernelArguments
            {
                ["query"] = message,
                ["conversationHistory"] = _conversationManager.GetConversationContext(chatHistory, maxMessages: 10, includeSystemPrompt: false),
                ["userId"] = userId
            };
            
            Console.WriteLine($"ChatProcessor calling SearchAgent.AnswerMovieQuery with contextual query: '{message}'");
            var searchResult = await searchAgent["AnswerMovieQuery"].InvokeAsync(kernel, searchArguments);
            Console.WriteLine($"SearchAgent returned: {searchResult}");
            
            // Parse the search result to get the intelligent response
            var searchResponse = JsonSerializer.Deserialize<MovieResponse>(searchResult.ToString());
            var intelligentResponse = searchResponse?.IntelligentResponse ?? "I couldn't process your movie query.";
            
            // Enhance the response with conversation context if needed
            var contextualResponse = await EnhanceResponseWithContext(message, intelligentResponse, chatHistory);
            
            // Update the search response with the contextual response
            var finalResponse = new MovieResponse
            {
                SimilarMovies = searchResponse?.SimilarMovies ?? new List<Movie>(),
                IntelligentResponse = contextualResponse
            };
            
            // Add to conversation history
            _conversationManager.AddToConversation(userId, message, contextualResponse);
            
            return JsonSerializer.Serialize(finalResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ChatProcessor: {ex}");
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "I'm having trouble processing your movie request right now. Please try again.",
                Details = ex.Message
            });
        }
    }

    private async Task<bool> IsSpecificMovieQueryAsync(string message, ChatHistory chatHistory)
    {
        try
        {
            var conversationContext = _conversationManager.GetConversationContext(chatHistory, maxMessages: 4);
            
            var classificationPrompt = @"You are a query type classifier for a movie assistant. Determine if the user is asking about a SPECIFIC movie/film or making a GENERAL query for recommendations/searches.

SPECIFIC movie queries ask about particular films by name:
- ""Tell me about Star Wars""
- ""What's the plot of Inception?""
- ""Who directed The Matrix?""
- ""When was Titanic released?""
- ""What happens in that movie?"" (referring to previously mentioned film)

GENERAL queries ask for recommendations, categories, or broad searches:
- ""Show me action movies""
- ""Movies like Star Wars""
- ""Best sci-fi films""
- ""Horror movie recommendations""
- ""Movies with time travel""

Respond with ONLY 'SPECIFIC' for specific movie queries or 'GENERAL' for general/recommendation queries.

Recent conversation context:
" + conversationContext + @"

Current user message: """ + message + @"""

Classification:";

            var tempHistory = new ChatHistory();
            tempHistory.AddUserMessage(classificationPrompt);
            
            var response = await _chatService.GetChatMessageContentAsync(tempHistory);
            var result = response.Content?.Trim().ToUpper();
            
            var isSpecific = result == "SPECIFIC";
            Console.WriteLine($"[AGENTIC] Query type classification - '{message}' -> {(isSpecific ? "SPECIFIC" : "GENERAL")}");
            
            return isSpecific;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AGENTIC] Error in query type classification, defaulting to GENERAL: {ex.Message}");
            // Default to general search if classification fails
            return false;
        }
    }

    private async Task<bool> IsMovieRelatedAsync(string message, ChatHistory chatHistory)
    {
        try
        {
            var conversationContext = _conversationManager.GetConversationContext(chatHistory, maxMessages: 6);
            
            var classificationPrompt = @"You are a movie query classifier. Your job is to determine if a user's message is related to movies, films, TV shows, actors, directors, or entertainment content.

Consider the conversation context to better understand follow-up questions and references.

Respond with ONLY 'YES' if the query is movie/entertainment related, or 'NO' if it's not.

Examples:
- 'What's the weather like?' -> NO
- 'Recommend some action movies' -> YES  
- 'Tell me about The Matrix' -> YES
- 'What happens in Inception?' -> YES
- 'Who directed Pulp Fiction?' -> YES
- 'What's 2+2?' -> NO
- 'How do I cook pasta?' -> NO
- 'What are some good sci-fi films?' -> YES
- 'I need help with my homework' -> NO
- 'What's the plot of Interstellar?' -> YES
- 'What about the sequel?' (after discussing a movie) -> YES
- 'Who was the main character?' (in movie context) -> YES

Recent conversation context:
" + conversationContext + @"

Current user message: """ + message + @"""

Classification:";

            var tempHistory = new ChatHistory();
            tempHistory.AddUserMessage(classificationPrompt);
            
            var response = await _chatService.GetChatMessageContentAsync(tempHistory);
            var result = response.Content?.Trim().ToUpper();
            
            var isMovieRelated = result == "YES";
            Console.WriteLine($"LLM Classification with context - Query: '{message}' -> {(isMovieRelated ? "MOVIE-RELATED" : "NOT MOVIE-RELATED")}");
            
            return isMovieRelated;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in movie classification, defaulting to movie-related: {ex.Message}");
            // Default to true to avoid blocking legitimate movie queries if classification fails
            return true;
        }
    }

    private async Task<string> GenerateMovieSteeringResponseAsync(string originalQuery, ChatHistory chatHistory)
    {
        try
        {
            var conversationContext = _conversationManager.GetConversationContext(chatHistory, maxMessages: 4);
            
            var steeringPrompt = @"You are a friendly movie assistant. A user has asked you a question that is NOT related to movies, films, TV shows, or entertainment. Your job is to politely redirect them to movie-related topics while being helpful and engaging.

Consider the conversation history to provide a more personalized response.

Guidelines:
1. Acknowledge their question briefly
2. Explain that you specialize in movies
3. Try to connect their topic to movie themes if possible
4. Ask them what kind of movies they're interested in
5. Keep the tone friendly and conversational
6. Don't be too lengthy - 2-3 sentences max
7. If you've had previous conversations, reference them naturally

Recent conversation context:
" + conversationContext + @"

Current user query: """ + originalQuery + @"""

Your response:";

            var tempHistory = new ChatHistory();
            tempHistory.AddUserMessage(steeringPrompt);
            
            var response = await _chatService.GetChatMessageContentAsync(tempHistory);
            var steeringResponse = response.Content?.Trim() ?? 
                "I'm your friendly movie assistant! I specialize in helping you discover great films and learn about movie plots. What kind of movies are you interested in exploring today?";
            
            Console.WriteLine($"Generated contextual steering response for non-movie query: '{originalQuery}' -> '{steeringResponse}'");
            return steeringResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating steering response: {ex.Message}");
            // Fallback to a generic response
            return "I'm your friendly movie assistant! I specialize in helping you discover great films, learn about movie plots, and find recommendations based on your preferences. What kind of movies are you interested in exploring today?";
        }
    }

    private async Task<string> EnhanceResponseWithContext(string userMessage, string searchResponse, ChatHistory chatHistory)
    {
        try
        {
            // If conversation history is minimal, return the search response as-is
            if (chatHistory.Count <= 2) // Only system message + current user message
            {
                return searchResponse;
            }

            var conversationContext = _conversationManager.GetConversationContext(chatHistory, maxMessages: 6, includeSystemPrompt: false);
            
            var enhancementPrompt = @"You are enhancing a movie assistant's response by adding conversation context. Your job is to make the response more natural and contextual based on the conversation history.

Guidelines:
1. Keep the core information from the original response
2. Add natural references to previous conversation if relevant
3. Make the response flow better in the conversation context
4. Don't add unnecessary information
5. If the original response is already perfect, return it unchanged
6. Keep the same helpful and engaging tone

Recent conversation context:
" + conversationContext + @"

Current user message: """ + userMessage + @"""

Original response: """ + searchResponse + @"""

Enhanced response:";

            var tempHistory = new ChatHistory();
            tempHistory.AddUserMessage(enhancementPrompt);
            
            var response = await _chatService.GetChatMessageContentAsync(tempHistory);
            var enhancedResponse = response.Content?.Trim() ?? searchResponse;
            
            Console.WriteLine($"Enhanced response with conversation context");
            return enhancedResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enhancing response with context: {ex.Message}");
            return searchResponse; // Fallback to original response
        }
    }

    /// <summary>
    /// Agentic approach using auto-function calling
    /// The kernel automatically decides which functions to invoke based on user intent
    /// </summary>
    public async Task<string> ProcessMessageAgenticAsync(Kernel kernel, string message, string userId)
    {
        Console.WriteLine($"[AGENTIC-ENTRY] Method called - user: '{userId}', message: '{message}'");
        try
        {
            Console.WriteLine($"[AGENTIC] Starting agentic processing for user '{userId}': '{message}'");
            
            // Get or create conversation history for this user
            var chatHistory = _conversationManager.GetOrCreateUserConversation(userId, _systemPrompt);
            Console.WriteLine($"[AGENTIC] Chat history initialized. Message count: {chatHistory.Count}");
            
            Console.WriteLine($"[AGENTIC] Proceeding with FULLY agentic processing - AI will decide how to handle the query...");
            
            // Add the user message to chat history
            chatHistory.AddUserMessage(message);
            Console.WriteLine($"[AGENTIC] Added user message to chat history. New count: {chatHistory.Count}");
            
            // Configure auto-function calling settings - let AI choose ALL functions
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
            Console.WriteLine($"[AGENTIC] Execution settings configured with FULL auto-function calling");
            
            // List available plugins and functions for debugging
            Console.WriteLine($"[AGENTIC] Available functions for AI to choose from:");
            foreach (var plugin in kernel.Plugins)
            {
                Console.WriteLine($"[AGENTIC]   Plugin: {plugin.Name}");
                foreach (var function in plugin)
                {
                    Console.WriteLine($"[AGENTIC]     Function: {function.Name} - {function.Description}");
                }
            }
            
            Console.WriteLine($"[AGENTIC] Letting AI choose which functions to call...");
            
            // Let the AI decide which functions to call from ALL available functions
            var aiResponse = await _chatService.GetChatMessageContentAsync(
                chatHistory, 
                executionSettings, 
                kernel);
            
            Console.WriteLine($"[AGENTIC] AI completed function calling and returned response");
            Console.WriteLine($"[AGENTIC] AI response content: '{aiResponse.Content}'");
            
            var intelligentResponse = aiResponse.Content ?? "I couldn't process your query.";
            
            // Add assistant response to conversation history
            chatHistory.AddAssistantMessage(intelligentResponse);
            Console.WriteLine($"[AGENTIC] Added AI response to chat history");
            
            // Add to conversation manager
            _conversationManager.AddToConversation(userId, message, intelligentResponse);
            Console.WriteLine($"[AGENTIC] Added to conversation manager");
            
            // For fully agentic mode, we focus on the AI's intelligent response
            var finalResponse = JsonSerializer.Serialize(new MovieResponse 
            { 
                SimilarMovies = new List<Movie>(), // AI-driven approach focuses on conversational response
                IntelligentResponse = intelligentResponse
            });
            
            Console.WriteLine($"[AGENTIC] Final response JSON length: {finalResponse.Length}");
            Console.WriteLine($"[AGENTIC] FULLY agentic processing completed successfully");
            
            return finalResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AGENTIC] ERROR in agentic ChatProcessor: {ex}");
            Console.WriteLine($"[AGENTIC] ERROR Stack trace: {ex.StackTrace}");
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Error = "InternalServerError", 
                Message = "I'm having trouble processing your movie request right now. Please try again.",
                Details = ex.Message
            });
        }
    }
} 