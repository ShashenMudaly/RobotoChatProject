using System.Threading.Tasks;

namespace ChatApp.Services.Interfaces;

public interface IContextStrategy
{
    Task<string> BuildContext(object contextData, string query);
} 