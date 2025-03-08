# RobotoAPI - Movie Conversation System

A sophisticated conversational AI system specialized in discussing movies. The system maintains context-aware conversations about movies, handling various types of queries including direct movie references, movie descriptions, and follow-up questions.

## Features

- **Context-Aware Conversations**: Maintains conversation context while allowing natural topic switches
- **Multiple Query Types Support**:
  - Direct movie name mentions
  - Movie descriptions without names
  - Follow-up questions about previously discussed movies
  - General movie-related queries
- **Smart Context Building**: Automatically detects and includes relevant movie information in conversations
- **Conversation History**: Maintains chat history for contextual responses

## System Components

### Core Services

- `MovieConversationOrchestrator`: Main orchestrator handling query processing and context management
- `ChatClient`: Handles AI-powered chat interactions and intent detection
- `MovieSearchService`: Manages movie lookups and similarity searches
- `ChatCacheRepository`: Handles conversation history storage and retrieval
- `TextSummarizationService`: Provides text summarization capabilities

### Key Features

1. **Context Management**
   - Maintains conversation flow
   - Detects context switches
   - Preserves movie information for follow-ups

2. **Movie Detection**
   - Direct name detection in queries
   - Movie reference detection in conversation history
   - Similar movie matching based on descriptions

3. **Response Generation**
   - Context-aware responses
   - Movie plot integration
   - Conversation history consideration

## Usage

The system processes queries through the `ProcessQuery` method:

```csharp
var (response, context) = await movieOrchestrator.ProcessQuery(userId, query);
```

### Query Types Handled

1. **Direct Movie Questions**
   ```
   "Tell me about The Matrix"
   ```

2. **Description-Based Queries**
   ```
   "What's that movie about a guy who discovers he's living in a simulation?"
   ```

3. **Follow-up Questions**
   ```
   "Who directed it?"
   "What year was it released?"
   ```

## Dependencies

- .NET Core 6.0+
- AI/ML services for natural language processing
- Database system for chat history storage

## Configuration

Ensure the following services are properly configured:
- Chat AI service
- Movie database connection
- Cache repository
- Logging system

## Logging

The system includes comprehensive logging for:
- Query processing duration
- Context building steps
- Movie detection results
- Response generation metrics

## Best Practices

1. **Context Management**
   - Keep conversation history concise
   - Regularly clean up old contexts
   - Monitor context switching accuracy

2. **Performance**
   - Cache frequently accessed movie information
   - Optimize movie search operations
   - Monitor response times

3. **Error Handling**
   - Graceful fallbacks for missing movie information
   - Proper exception handling and logging
   - User-friendly error messages

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[Specify your license here] 