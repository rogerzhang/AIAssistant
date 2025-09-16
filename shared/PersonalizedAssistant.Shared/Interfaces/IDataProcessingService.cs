using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.Shared.Interfaces;

public interface IDataProcessingService
{
    Task<bool> ProcessDataAsync(DataCollection data);
    Task<bool> ProcessUserPreferencesAsync(string userId);
    Task<UserPreferences?> GetUserPreferencesAsync(string userId);
    Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences);
    Task<List<Insight>> GenerateInsightsAsync(string userId);
    Task<bool> TrainModelAsync(string userId);
    Task<ProcessingResult> ProcessBatchAsync(List<DataCollection> dataCollection);
}

public class Insight
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

