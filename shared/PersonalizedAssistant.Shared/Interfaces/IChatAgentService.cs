using PersonalizedAssistant.Shared.Models;

namespace PersonalizedAssistant.Shared.Interfaces;

public interface IChatAgentService
{
    Task<ChatResponse> ProcessMessageAsync(string userId, string message, string? sessionId = null);
    Task<ChatSession> CreateSessionAsync(string userId);
    Task<ChatSession?> GetSessionAsync(string sessionId);
    Task<bool> UpdateSessionAsync(ChatSession session);
    Task<List<ChatSession>> GetUserSessionsAsync(string userId, int limit = 10);
    Task<bool> DeleteSessionAsync(string sessionId);
    Task<List<string>> GetSuggestedQuestionsAsync(string userId);
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double Confidence { get; set; } = 0.0;
}

