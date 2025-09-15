using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.Shared.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("googleId")]
    public string GoogleId { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("profilePicture")]
    public string? ProfilePicture { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("permissions")]
    public List<DataPermission> Permissions { get; set; } = new();

    [BsonElement("preferences")]
    public UserPreferences? Preferences { get; set; }
}

public class DataPermission
{
    [BsonElement("source")]
    public DataSource Source { get; set; }

    [BsonElement("grantedAt")]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("scope")]
    public List<string> Scope { get; set; } = new();
}

public class UserPreferences
{
    [BsonElement("interests")]
    public List<string> Interests { get; set; } = new();

    [BsonElement("habits")]
    public List<string> Habits { get; set; } = new();

    [BsonElement("relationships")]
    public List<Relationship> Relationships { get; set; } = new();

    [BsonElement("tasks")]
    public List<TaskItem> Tasks { get; set; } = new();

    [BsonElement("healthInfo")]
    public HealthInfo? HealthInfo { get; set; }

    [BsonElement("workInfo")]
    public WorkInfo? WorkInfo { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class Relationship
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // friend, family, colleague, doctor, etc.

    [BsonElement("contactInfo")]
    public ContactInfo? ContactInfo { get; set; }

    [BsonElement("source")]
    public DataSource Source { get; set; }

    [BsonElement("confidence")]
    public double Confidence { get; set; } = 0.0;
}

public class TaskItem
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("dueDate")]
    public DateTime? DueDate { get; set; }

    [BsonElement("priority")]
    public string Priority { get; set; } = "Medium";

    [BsonElement("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [BsonElement("source")]
    public DataSource Source { get; set; }
}

public class HealthInfo
{
    [BsonElement("doctors")]
    public List<Doctor> Doctors { get; set; } = new();

    [BsonElement("medications")]
    public List<string> Medications { get; set; } = new();

    [BsonElement("allergies")]
    public List<string> Allergies { get; set; } = new();
}

public class Doctor
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("specialty")]
    public string Specialty { get; set; } = string.Empty;

    [BsonElement("contactInfo")]
    public ContactInfo? ContactInfo { get; set; }

    [BsonElement("lastVisit")]
    public DateTime? LastVisit { get; set; }
}

public class WorkInfo
{
    [BsonElement("company")]
    public string? Company { get; set; }

    [BsonElement("position")]
    public string? Position { get; set; }

    [BsonElement("colleagues")]
    public List<Relationship> Colleagues { get; set; } = new();

    [BsonElement("projects")]
    public List<string> Projects { get; set; } = new();
}

public class ContactInfo
{
    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("organization")]
    public string? Organization { get; set; }
}
