using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.Shared.Interfaces;

public interface IDataCollectionService
{
    Task<bool> CollectGmailDataAsync(string userId, string accessToken);
    Task<bool> CollectGoogleDriveDataAsync(string userId, string accessToken);
    Task<bool> CollectIOSContactsDataAsync(string userId, List<IOSContactData> contacts);
    Task<bool> CollectIOSCalendarDataAsync(string userId, List<IOSCalendarData> events);
    Task<List<DataCollection>> GetCollectedDataAsync(string userId, DataSource? source = null);
    Task<DataCollection?> GetDataByIdAsync(string dataId);
    Task<bool> UpdateDataStatusAsync(string dataId, ProcessingStatus status, string? errorMessage = null);
    Task<bool> DeleteDataAsync(string dataId);
    Task<List<DataCollection>> GetPendingDataAsync(int limit = 100);
}

