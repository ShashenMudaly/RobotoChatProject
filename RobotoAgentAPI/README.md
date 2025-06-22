# Movie Chat API

A sophisticated multi-agent RESTful movie chat API built with .NET 8, ASP.NET Core, and Semantic Kernel. The system uses Azure OpenAI for intelligent conversation and Bono Search API for movie data retrieval, featuring advanced conversation history and context-aware movie recommendations.

## 🎬 Features

### Core Functionality
- **Multi-agent architecture** with specialized ChatAgent and SearchAgent
- **Conversation history management** - maintains context across user sessions
- **Context-aware movie queries** - understands follow-up questions about recently discussed movies
- **Intelligent movie search routing** - extracts movie names for targeted searches vs. hybrid search
- **Movie-focused conversations** - politely redirects non-movie queries with contextual responses

### Technical Features
- **Semantic Kernel** for agent orchestration and plugin management
- **Azure OpenAI integration** with intelligent query classification
- **Bono Search API** with hybrid search and specific movie lookup
- **Rate limiting** (100 requests/minute per IP)
- **CORS support** for web applications
- **Swagger/OpenAPI documentation**
- **Modular architecture** with separated concerns

## 🏗️ Architecture

### Agent System
- **`ChatAgentPlugin`** - Main orchestrator exposing kernel functions
- **`ChatProcessor`** - Core chat logic, movie classification, and response enhancement
- **`ConversationManager`** - Thread-safe conversation history management
- **`SearchAgentPlugin`** - Intelligent movie search with context awareness

### Smart Query Routing
```
User Query → Extract Movie Names
    ↓
If movie names found → Direct Movie Search
    ↓
If no movie names → Check Recent Movie Context
    ↓
If related to recent movie → Answer using movie context
    ↓
If not related → Hybrid Search
```

## 📋 Prerequisites

- .NET 8 SDK
- Azure OpenAI account with deployment "o3-mini"
- Bono Search API account

## ⚙️ Configuration

1. Update `appsettings.json` with your credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "YOUR_AZURE_OPENAI_ENDPOINT",
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY"
  },
  "BonoSearch": {
    "Endpoint": "YOUR_BONO_SEARCH_ENDPOINT",
    "ApiKey": "YOUR_BONO_SEARCH_API_KEY"
  }
}
```

## 🚀 Running Locally

1. Clone the repository
2. Update the configuration in `appsettings.json`
3. Run the following commands:

```bash
dotnet restore
dotnet build
dotnet run
```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

Alternatively, use the PowerShell scripts:
```powershell
.\run-dev.ps1      # Start development server
.\test-api.ps1     # Test the API
.\test-health.ps1  # Check health endpoint
```

## 🔌 API Usage

### Chat Endpoint (with User Sessions)

```http
POST /api/chat
Content-Type: application/json

{
    "message": "What's the plot of Fight Club?",
    "userId": "user123"  // Optional: for conversation history
}
```

### Follow-up Queries (Context-Aware)

```http
POST /api/chat
Content-Type: application/json

{
    "message": "Was Tyler real?",  // Understands this relates to Fight Club
    "userId": "user123"
}
```

### Enhanced Response Format

```json
{
    "similarMovies": [
        {
            "id": "string",
            "title": "string",
            "year": number,
            "similarityScore": number,
            "posterUrl": "string",
            "plot": "string"
        }
    ],
    "intelligentResponse": "Detailed AI-generated response based on movie context and conversation history"
}
```

### Conversation Management

```http
# Clear conversation history
POST /api/chat/clear-history
Content-Type: application/json
{
    "userId": "user123"
}

# Get conversation summary
GET /api/chat/summary?userId=user123
```

### Error Response

```json
{
    "error": "string",
    "message": "string",
    "details": "string"
}
```

## 🎯 Intelligent Features

### Context-Aware Conversations
The system remembers previous movies discussed and can answer follow-up questions:

**Example Conversation:**
1. User: "What's the plot of Inception?"
2. System: *[Provides Inception plot]*
3. User: "How many dream levels were there?" 
4. System: *[Understands this relates to Inception and answers specifically]*

### Smart Query Classification
- **Movie Names Detected**: Direct search using movie endpoint
- **No Movie Names + Recent Context**: Uses conversation history to answer
- **General Movie Query**: Hybrid search for recommendations
- **Non-Movie Query**: Polite redirection with movie suggestions

### Conversation History Management
- **Per-user session tracking** with configurable user IDs
- **Automatic history trimming** (20 message pairs max)
- **Context extraction** for AI decision making
- **Thread-safe concurrent user handling**

## 📁 Project Structure

```
RobotoAgentAPI/
├── Controllers/
│   └── ChatController.cs           # API endpoint handler
├── Agents/
│   ├── ChatAgentPlugin.cs         # Main kernel function orchestrator
│   ├── ChatProcessor.cs           # Core chat processing logic
│   ├── ConversationManager.cs     # Conversation history management
│   └── SearchAgentPlugin.cs       # Movie search with context awareness
├── Prompts/
│   └── MovieAgentPrompt.txt       # System prompt for ChatAgent
├── Program.cs                     # Application configuration
├── appsettings.json              # Configuration settings
├── .gitignore                    # Git ignore patterns
└── README.md                     # This file
```

## 🎛️ Rate Limiting

- 100 requests per minute per IP address
- Rate limit headers:
  - `X-RateLimit-Limit`
  - `X-RateLimit-Remaining`  
  - `X-RateLimit-Reset`

## 🧪 Testing

### Using cURL

```bash
# Basic movie query
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Tell me about The Matrix", "userId": "test-user"}'

# Follow-up query (context-aware)
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \  
  -d '{"message": "Who was the main character?", "userId": "test-user"}'

# Clear conversation history
curl -X POST https://localhost:5001/api/chat/clear-history \
  -H "Content-Type: application/json" \
  -d '{"userId": "test-user"}'
```

### Using PowerShell Scripts

```powershell
.\test-api.ps1      # Automated API testing
.\test-health.ps1   # Health check testing
```

## 🛠️ Customizing the System

### System Prompt
The system prompt can be modified in `/Prompts/MovieAgentPrompt.txt`. The prompt enforces:
- Movie-focused conversation guidelines
- Response format requirements
- Context awareness instructions
- Error handling behavior

### Conversation History Settings
In `ConversationManager.cs`:
- `_maxHistoryLength` - Maximum message pairs to retain (default: 20)
- History trimming behavior
- Context extraction parameters

### Search Behavior
In `SearchAgentPlugin.cs`:
- Movie name extraction prompts
- Context relationship analysis
- Search routing logic
- Response enhancement settings

## 🔧 Error Handling

The API returns appropriate HTTP status codes:
- **200**: Successful response with movie data
- **400**: Bad request (invalid input)
- **429**: Too many requests (rate limited)
- **500**: Internal server error

Comprehensive error handling includes:
- Graceful degradation when AI services fail
- Fallback search mechanisms
- Detailed error logging for debugging
- User-friendly error messages

## 🚦 Development Features

### Logging
- Comprehensive console logging for debugging
- Query classification decisions
- Search routing choices
- Context analysis results
- API response truncation (first 100 chars)

### Debugging Tools
- Conversation history inspection
- Query routing decision logging
- AI classification confidence tracking
- Search strategy selection logging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **Semantic Kernel** for agent orchestration
- **Azure OpenAI** for intelligent conversation capabilities
- **Bono Search API** for comprehensive movie data 