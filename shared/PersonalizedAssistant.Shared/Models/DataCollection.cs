using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.Shared.Models;

public class DataCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("source")]
    public DataSource Source { get; set; }

    [BsonElement("dataType")]
    public string DataType { get; set; } = string.Empty; // email, contact, calendar, file, etc.

    [BsonElement("rawData")]
    public string RawData { get; set; } = string.Empty;

    [BsonElement("processedData")]
    public Dictionary<string, object> ProcessedData { get; set; } = new();

    [BsonElement("status")]
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;

    [BsonElement("collectedAt")]
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class GmailData : DataCollection
{
    [BsonElement("subject")]
    public string Subject { get; set; } = string.Empty;

    [BsonElement("sender")]
    public string Sender { get; set; } = string.Empty;

    [BsonElement("recipients")]
    public List<string> Recipients { get; set; } = new();

    [BsonElement("sentAt")]
    public DateTime SentAt { get; set; }

    [BsonElement("threadId")]
    public string ThreadId { get; set; } = string.Empty;

    [BsonElement("labels")]
    public List<string> Labels { get; set; } = new();
}

public class GoogleDriveData : DataCollection
{
    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("fileType")]
    public string FileType { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; }

    [BsonElement("folderPath")]
    public string FolderPath { get; set; } = string.Empty;

    [BsonElement("lastModified")]
    public DateTime LastModified { get; set; }

    [BsonElement("sharedWith")]
    public List<string> SharedWith { get; set; } = new();
}

public class IOSContactData : DataCollection
{
    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("emails")]
    public List<string> Emails { get; set; } = new();

    [BsonElement("phones")]
    public List<string> Phones { get; set; } = new();

    [BsonElement("organization")]
    public string? Organization { get; set; }

    [BsonElement("jobTitle")]
    public string? JobTitle { get; set; }
}

public class IOSCalendarData : DataCollection
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("startTime")]
    public DateTime StartTime { get; set; }

    [BsonElement("endTime")]
    public DateTime EndTime { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("attendees")]
    public List<string> Attendees { get; set; } = new();

    [BsonElement("isAllDay")]
    public bool IsAllDay { get; set; } = false;
}

