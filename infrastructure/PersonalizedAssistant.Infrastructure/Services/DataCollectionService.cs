using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalizedAssistant.Infrastructure.Configuration;
using PersonalizedAssistant.Infrastructure.Data;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace PersonalizedAssistant.Infrastructure.Services;

public class DataCollectionService : IDataCollectionService
{
    private readonly MongoDbContext _context;
    private readonly AppSettings _settings;
    private readonly ILogger<DataCollectionService> _logger;

    public DataCollectionService(MongoDbContext context, IOptions<AppSettings> settings, ILogger<DataCollectionService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> CollectGmailDataAsync(string userId, string accessToken)
    {
        try
        {
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = new GoogleCredentialAccessToken(accessToken),
                ApplicationName = "Personalized Assistant"
            });

            var request = service.Users.Messages.List("me");
            request.MaxResults = 100; // Limit for POC

            var response = await request.ExecuteAsync();
            var collectedCount = 0;

            if (response.Messages != null)
            {
                foreach (var message in response.Messages)
                {
                    var messageRequest = service.Users.Messages.Get("me", message.Id);
                    var messageResponse = await messageRequest.ExecuteAsync();

                    var gmailData = new GmailData
                    {
                        UserId = userId,
                        Source = DataSource.Gmail,
                        DataType = "email",
                        RawData = System.Text.Json.JsonSerializer.Serialize(messageResponse),
                        Status = ProcessingStatus.Pending,
                        CollectedAt = DateTime.UtcNow,
                        Subject = ExtractHeader(messageResponse.Payload.Headers, "Subject") ?? "",
                        Sender = ExtractHeader(messageResponse.Payload.Headers, "From") ?? "",
                        SentAt = DateTimeOffset.FromUnixTimeMilliseconds(messageResponse.InternalDate ?? 0).DateTime,
                        ThreadId = messageResponse.ThreadId ?? "",
                        Labels = messageResponse.LabelIds?.ToList() ?? new List<string>()
                    };

                    await _context.GmailData.InsertOneAsync(gmailData);
                    collectedCount++;
                }
            }

            _logger.LogInformation("Collected {Count} Gmail messages for user: {UserId}", collectedCount, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Gmail data for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CollectGoogleDriveDataAsync(string userId, string accessToken)
    {
        try
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = new GoogleCredentialAccessToken(accessToken),
                ApplicationName = "Personalized Assistant"
            });

            var request = service.Files.List();
            request.PageSize = 100; // Limit for POC
            request.Fields = "files(id,name,mimeType,size,modifiedTime,parents,sharedWithMe)";

            var response = await request.ExecuteAsync();
            var collectedCount = 0;

            if (response.Files != null)
            {
                foreach (var file in response.Files)
                {
                    var driveData = new GoogleDriveData
                    {
                        UserId = userId,
                        Source = DataSource.GoogleDrive,
                        DataType = "file",
                        RawData = System.Text.Json.JsonSerializer.Serialize(file),
                        Status = ProcessingStatus.Pending,
                        CollectedAt = DateTime.UtcNow,
                        FileName = file.Name ?? "",
                        FileType = file.MimeType ?? "",
                        FileSize = file.Size ?? 0,
                        LastModified = file.ModifiedTimeDateTimeOffset?.DateTime ?? DateTime.UtcNow,
                        SharedWith = file.Shared == true ? new List<string> { "Shared" } : new List<string>()
                    };

                    await _context.GoogleDriveData.InsertOneAsync(driveData);
                    collectedCount++;
                }
            }

            _logger.LogInformation("Collected {Count} Google Drive files for user: {UserId}", collectedCount, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Google Drive data for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CollectIOSContactsDataAsync(string userId, List<IOSContactData> contacts)
    {
        try
        {
            var collectedCount = 0;

            foreach (var contact in contacts)
            {
                contact.UserId = userId;
                contact.Source = DataSource.IOSContacts;
                contact.DataType = "contact";
                contact.Status = ProcessingStatus.Pending;
                contact.CollectedAt = DateTime.UtcNow;

                await _context.IOSContactData.InsertOneAsync(contact);
                collectedCount++;
            }

            _logger.LogInformation("Collected {Count} iOS contacts for user: {UserId}", collectedCount, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting iOS contacts data for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CollectIOSCalendarDataAsync(string userId, List<IOSCalendarData> events)
    {
        try
        {
            var collectedCount = 0;

            foreach (var calendarEvent in events)
            {
                calendarEvent.UserId = userId;
                calendarEvent.Source = DataSource.IOSCalendar;
                calendarEvent.DataType = "calendar_event";
                calendarEvent.Status = ProcessingStatus.Pending;
                calendarEvent.CollectedAt = DateTime.UtcNow;

                await _context.IOSCalendarData.InsertOneAsync(calendarEvent);
                collectedCount++;
            }

            _logger.LogInformation("Collected {Count} iOS calendar events for user: {UserId}", collectedCount, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting iOS calendar data for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<DataCollection>> GetCollectedDataAsync(string userId, DataSource? source = null)
    {
        var filter = Builders<DataCollection>.Filter.Eq(d => d.UserId, userId);
        
        if (source.HasValue)
        {
            filter = Builders<DataCollection>.Filter.And(
                filter,
                Builders<DataCollection>.Filter.Eq(d => d.Source, source.Value)
            );
        }

        return await _context.DataCollections.Find(filter).ToListAsync();
    }

    public async Task<DataCollection?> GetDataByIdAsync(string dataId)
    {
        var filter = Builders<DataCollection>.Filter.Eq(d => d.Id, dataId);
        return await _context.DataCollections.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateDataStatusAsync(string dataId, ProcessingStatus status, string? errorMessage = null)
    {
        try
        {
            var filter = Builders<DataCollection>.Filter.Eq(d => d.Id, dataId);
            var update = Builders<DataCollection>.Update
                .Set(d => d.Status, status)
                .Set(d => d.ProcessedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                update = update.Set(d => d.ErrorMessage, errorMessage);
            }

            var result = await _context.DataCollections.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data status for data: {DataId}", dataId);
            return false;
        }
    }

    public async Task<bool> DeleteDataAsync(string dataId)
    {
        try
        {
            var filter = Builders<DataCollection>.Filter.Eq(d => d.Id, dataId);
            var result = await _context.DataCollections.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data: {DataId}", dataId);
            return false;
        }
    }

    public async Task<List<DataCollection>> GetPendingDataAsync(int limit = 100)
    {
        var filter = Builders<DataCollection>.Filter.Eq(d => d.Status, ProcessingStatus.Pending);
        return await _context.DataCollections.Find(filter).Limit(limit).ToListAsync();
    }

    private static string? ExtractHeader(IList<Google.Apis.Gmail.v1.Data.MessagePartHeader> headers, string name)
    {
        return headers?.FirstOrDefault(h => h.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)?.Value;
    }
}

public class GoogleCredentialAccessToken : Google.Apis.Auth.OAuth2.ICredential, Google.Apis.Http.IConfigurableHttpClientInitializer
{
    private readonly string _accessToken;

    public GoogleCredentialAccessToken(string accessToken)
    {
        _accessToken = accessToken;
    }

    public void Initialize(Google.Apis.Services.BaseClientService.Initializer initializer) { }

    public Task<string> GetAccessTokenForRequestAsync(string? authUri = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_accessToken);
    }

    public void Initialize(Google.Apis.Http.ConfigurableHttpClient httpClient)
    {
        // Implementation for IConfigurableHttpClientInitializer
    }
}
