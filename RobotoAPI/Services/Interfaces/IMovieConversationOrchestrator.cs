using System.Threading.Tasks;
using ChatApp.Services.Models;

namespace ChatApp.Services.Interfaces;

public interface IMovieConversationOrchestrator
{
    Task<QueryResult> ProcessQuery(string userId, string query);
} 