using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalizedAssistant.Infrastructure.Configuration;
using PersonalizedAssistant.Infrastructure.Data;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PersonalizedAssistant.Infrastructure.Services;

public class DataProcessingService : IDataProcessingService
{
    private readonly MongoDbContext _context;
    private readonly AppSettings _settings;
    private readonly ILogger<DataProcessingService> _logger;

    public DataProcessingService(MongoDbContext context, IOptions<AppSettings> settings, ILogger<DataProcessingService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> ProcessDataAsync(DataCollection data)
    {
        try
        {
            _logger.LogInformation("Processing data: {DataId} of type: {DataType}", data.Id, data.DataType);

            var processedData = new Dictionary<string, object>();

            switch (data.Source)
            {
                case DataSource.Gmail:
                    processedData = await ProcessGmailDataAsync(data);
                    break;
                case DataSource.GoogleDrive:
                    processedData = await ProcessGoogleDriveDataAsync(data);
                    break;
                case DataSource.IOSContacts:
                    processedData = await ProcessIOSContactsDataAsync(data);
                    break;
                case DataSource.IOSCalendar:
                    processedData = await ProcessIOSCalendarDataAsync(data);
                    break;
                default:
                    _logger.LogWarning("Unknown data source: {Source}", data.Source);
                    return false;
            }

            // Update the data collection with processed information
            var filter = Builders<DataCollection>.Filter.Eq(d => d.Id, data.Id);
            var update = Builders<DataCollection>.Update
                .Set(d => d.ProcessedData, processedData)
                .Set(d => d.Status, ProcessingStatus.Completed)
                .Set(d => d.ProcessedAt, DateTime.UtcNow);

            await _context.DataCollections.UpdateOneAsync(filter, update);

            // Update user preferences based on processed data
            await ProcessUserPreferencesAsync(data.UserId);

            _logger.LogInformation("Successfully processed data: {DataId}", data.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data: {DataId}", data.Id);
            
            var filter = Builders<DataCollection>.Filter.Eq(d => d.Id, data.Id);
            var update = Builders<DataCollection>.Update
                .Set(d => d.Status, ProcessingStatus.Failed)
                .Set(d => d.ErrorMessage, ex.Message)
                .Set(d => d.ProcessedAt, DateTime.UtcNow);

            await _context.DataCollections.UpdateOneAsync(filter, update);
            return false;
        }
    }

    public async Task<bool> ProcessUserPreferencesAsync(string userId)
    {
        try
        {
            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return false;
            }

            var preferences = await AnalyzeUserDataAsync(userId);
            if (preferences == null)
            {
                preferences = new UserPreferences();
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.Preferences, preferences)
                .Set(u => u.Preferences!.LastUpdated, DateTime.UtcNow);

            await _context.Users.UpdateOneAsync(filter, update);

            _logger.LogInformation("Updated preferences for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user preferences for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserPreferences?> GetUserPreferencesAsync(string userId)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.Preferences;
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.Preferences, preferences)
                .Set(u => u.Preferences.LastUpdated, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<Insight>> GenerateInsightsAsync(string userId)
    {
        var insights = new List<Insight>();
        var preferences = await GetUserPreferencesAsync(userId);

        if (preferences == null)
        {
            return insights;
        }

        // Generate insights based on user preferences
        if (preferences.Interests.Any())
        {
            insights.Add(new Insight
            {
                Type = "Interests",
                Description = $"You have {preferences.Interests.Count} identified interests: {string.Join(", ", preferences.Interests.Take(5))}",
                Confidence = 0.8,
                Data = new Dictionary<string, object> { { "interests", preferences.Interests } }
            });
        }

        if (preferences.Relationships.Any())
        {
            insights.Add(new Insight
            {
                Type = "Relationships",
                Description = $"You have {preferences.Relationships.Count} identified relationships",
                Confidence = 0.9,
                Data = new Dictionary<string, object> { { "relationships", preferences.Relationships } }
            });
        }

        if (preferences.Tasks.Any())
        {
            var pendingTasks = preferences.Tasks.Count(t => !t.IsCompleted);
            insights.Add(new Insight
            {
                Type = "Tasks",
                Description = $"You have {pendingTasks} pending tasks",
                Confidence = 1.0,
                Data = new Dictionary<string, object> { { "pending_tasks", pendingTasks } }
            });
        }

        return insights;
    }

    public async Task<bool> TrainModelAsync(string userId)
    {
        // Placeholder for ML model training
        // In a real implementation, this would train models based on user data
        _logger.LogInformation("Training model for user: {UserId}", userId);
        await Task.Delay(1000); // Simulate training time
        return true;
    }

    public async Task<ProcessingResult> ProcessBatchAsync(List<DataCollection> dataCollection)
    {
        var result = new ProcessingResult();
        var startTime = DateTime.UtcNow;

        foreach (var data in dataCollection)
        {
            try
            {
                var success = await ProcessDataAsync(data);
                if (success)
                {
                    result.ProcessedCount++;
                }
                else
                {
                    result.FailedCount++;
                    result.Errors.Add($"Failed to process data: {data.Id}");
                }
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Exception processing data {data.Id}: {ex.Message}");
            }
        }

        result.Success = result.FailedCount == 0;
        result.ProcessingTime = DateTime.UtcNow - startTime;

        return result;
    }

    private async Task<Dictionary<string, object>> ProcessGmailDataAsync(DataCollection data)
    {
        var processedData = new Dictionary<string, object>();

        try
        {
            var gmailData = JsonSerializer.Deserialize<GmailData>(data.RawData);
            if (gmailData != null)
            {
                processedData["subject"] = gmailData.Subject;
                processedData["sender"] = gmailData.Sender;
                processedData["sent_at"] = gmailData.SentAt;
                processedData["labels"] = gmailData.Labels;
                
                // Extract keywords from subject and content
                var keywords = ExtractKeywords(gmailData.Subject);
                processedData["keywords"] = keywords;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Gmail data: {DataId}", data.Id);
        }

        return processedData;
    }

    private async Task<Dictionary<string, object>> ProcessGoogleDriveDataAsync(DataCollection data)
    {
        var processedData = new Dictionary<string, object>();

        try
        {
            var driveData = JsonSerializer.Deserialize<GoogleDriveData>(data.RawData);
            if (driveData != null)
            {
                processedData["file_name"] = driveData.FileName;
                processedData["file_type"] = driveData.FileType;
                processedData["file_size"] = driveData.FileSize;
                processedData["last_modified"] = driveData.LastModified;
                
                // Extract file category based on type
                var category = CategorizeFile(driveData.FileType);
                processedData["category"] = category;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Google Drive data: {DataId}", data.Id);
        }

        return processedData;
    }

    private async Task<Dictionary<string, object>> ProcessIOSContactsDataAsync(DataCollection data)
    {
        var processedData = new Dictionary<string, object>();

        try
        {
            var contactData = JsonSerializer.Deserialize<IOSContactData>(data.RawData);
            if (contactData != null)
            {
                processedData["name"] = $"{contactData.FirstName} {contactData.LastName}".Trim();
                processedData["emails"] = contactData.Emails;
                processedData["phones"] = contactData.Phones;
                processedData["organization"] = contactData.Organization;
                processedData["job_title"] = contactData.JobTitle;
                
                // Determine relationship type
                var relationshipType = DetermineRelationshipType(contactData);
                processedData["relationship_type"] = relationshipType;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing iOS contacts data: {DataId}", data.Id);
        }

        return processedData;
    }

    private async Task<Dictionary<string, object>> ProcessIOSCalendarDataAsync(DataCollection data)
    {
        var processedData = new Dictionary<string, object>();

        try
        {
            var calendarData = JsonSerializer.Deserialize<IOSCalendarData>(data.RawData);
            if (calendarData != null)
            {
                processedData["title"] = calendarData.Title;
                processedData["description"] = calendarData.Description;
                processedData["start_time"] = calendarData.StartTime;
                processedData["end_time"] = calendarData.EndTime;
                processedData["location"] = calendarData.Location;
                processedData["attendees"] = calendarData.Attendees;
                processedData["is_all_day"] = calendarData.IsAllDay;
                
                // Extract event type
                var eventType = CategorizeEvent(calendarData.Title, calendarData.Description);
                processedData["event_type"] = eventType;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing iOS calendar data: {DataId}", data.Id);
        }

        return processedData;
    }

    private async Task<UserPreferences?> AnalyzeUserDataAsync(string userId)
    {
        var preferences = new UserPreferences();
        
        // Analyze Gmail data
        var gmailData = await _context.GmailData.Find(d => d.UserId == userId).ToListAsync();
        foreach (var email in gmailData)
        {
            // Extract interests from email subjects and content
            var keywords = ExtractKeywords(email.Subject);
            preferences.Interests.AddRange(keywords.Where(k => !preferences.Interests.Contains(k)));
        }

        // Analyze contacts data
        var contactsData = await _context.IOSContactData.Find(d => d.UserId == userId).ToListAsync();
        foreach (var contact in contactsData)
        {
            var relationship = new Relationship
            {
                Name = $"{contact.FirstName} {contact.LastName}".Trim(),
                Type = DetermineRelationshipType(contact),
                ContactInfo = new ContactInfo
                {
                    Email = contact.Emails.FirstOrDefault(),
                    Phone = contact.Phones.FirstOrDefault(),
                    Organization = contact.Organization
                },
                Source = DataSource.IOSContacts,
                Confidence = 0.8
            };
            preferences.Relationships.Add(relationship);
        }

        // Analyze calendar data
        var calendarData = await _context.IOSCalendarData.Find(d => d.UserId == userId).ToListAsync();
        foreach (var calendarEvent in calendarData)
        {
            var task = new TaskItem
            {
                Title = calendarEvent.Title,
                Description = calendarEvent.Description,
                DueDate = calendarEvent.StartTime,
                Priority = "Medium",
                IsCompleted = calendarEvent.EndTime < DateTime.UtcNow,
                Source = DataSource.IOSCalendar
            };
            preferences.Tasks.Add(task);
        }

        return preferences;
    }

    private List<string> ExtractKeywords(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        // Simple keyword extraction - in a real implementation, use NLP libraries
        var words = text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Where(w => !IsStopWord(w))
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();

        return words;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "its", "may", "new", "now", "old", "see", "two", "way", "who", "boy", "did", "man", "oil", "sit", "try", "use", "she", "use", "this", "that", "with", "have", "will", "your", "from", "they", "know", "want", "been", "good", "much", "some", "time", "very", "when", "come", "here", "just", "like", "long", "make", "many", "over", "such", "take", "than", "them", "well", "were" };
        return stopWords.Contains(word);
    }

    private string CategorizeFile(string mimeType)
    {
        return mimeType switch
        {
            var type when type.StartsWith("image/") => "Image",
            var type when type.StartsWith("video/") => "Video",
            var type when type.StartsWith("audio/") => "Audio",
            var type when type.Contains("pdf") => "Document",
            var type when type.Contains("word") => "Document",
            var type when type.Contains("excel") => "Spreadsheet",
            var type when type.Contains("powerpoint") => "Presentation",
            _ => "Other"
        };
    }

    private string DetermineRelationshipType(IOSContactData contact)
    {
        var name = $"{contact.FirstName} {contact.LastName}".ToLowerInvariant();
        var organization = contact.Organization?.ToLowerInvariant() ?? "";

        if (organization.Contains("doctor") || organization.Contains("medical") || organization.Contains("hospital"))
            return "Doctor";
        if (organization.Contains("company") || organization.Contains("corp") || organization.Contains("inc"))
            return "Colleague";
        if (name.Contains("mom") || name.Contains("dad") || name.Contains("mother") || name.Contains("father"))
            return "Family";
        
        return "Friend";
    }

    private string CategorizeEvent(string title, string? description)
    {
        var text = $"{title} {description}".ToLowerInvariant();
        
        if (text.Contains("meeting") || text.Contains("call"))
            return "Work";
        if (text.Contains("doctor") || text.Contains("medical") || text.Contains("appointment"))
            return "Health";
        if (text.Contains("birthday") || text.Contains("party") || text.Contains("celebration"))
            return "Personal";
        if (text.Contains("travel") || text.Contains("trip") || text.Contains("vacation"))
            return "Travel";
        
        return "Other";
    }
}
