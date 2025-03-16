# Movie Conversation AI

An intelligent chatbot system that engages in contextual conversations about movies, powered by Azure OpenAI.

## Features

- **Contextual Movie Conversations**: Maintains conversation context and understands movie-related queries
- **Smart Movie Detection**: Identifies movie names from user queries and conversation history
- **Conversation History**: Maintains chat history for improved context awareness
- **Multiple Context Strategies**:
  - Single Movie: Detailed information about a specific movie
  - Similar Movies: Comparisons and recommendations
  - Conversation: General movie discussions

## Prerequisites

- .NET 7.0 or later
- Azure OpenAI API access
- Azure Language Service (for text analysis)

## Configuration

### Azure OpenAI Configuration

Add your Azure OpenAI settings to `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "your-endpoint",
    "Key": "your-key",
    "DeploymentName": "your-deployment"
  }
}
```

## Running the Application

```bash
dotnet build
dotnet run
```

## Architecture

### Key Components

- **MovieConversationOrchestrator**: Manages conversation flow and context building
- **ChatClient**: Handles AI interactions using Azure OpenAI
- **Context Strategies**:
  - `SingleMovieStrategy`
  - `SimilarMoviesStrategy`
  - `ConversationStrategy`

### Data Flow

1. User query received
2. Movie intent checked with conversation context
3. Relevant movie information extracted
4. Context built using appropriate strategy
5. Response generated using Azure OpenAI
6. Conversation history maintained

## Development

### Adding New Features

1. **New Context Strategy**:
   - Implement `IContextStrategy`
   - Register in `ContextStrategyFactory`
   - Add to dependency injection in `Program.cs`

2. **Extending Chat Capabilities**:
   - Update `IChatClient` interface
   - Implement in `ChatClient`
   - Add necessary prompts and error handling

## Error Handling

The system includes comprehensive error handling:
- AI service fallbacks
- Conversation context recovery
- Detailed logging throughout

## Logging

Structured logging is implemented throughout the application:
- Movie detection events
- Context building process
- AI service interactions
- Query processing and responses

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[Your License Here] 