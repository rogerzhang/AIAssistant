using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PersonalizedAssistant.Shared.Models;

public class ChatSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [BsonElement("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastActivityAt")]
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("context")]
    public Dictionary<string, object> Context { get; set; } = new();
}

public class ChatMessage
{
    [BsonElement("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("role")]
    public MessageRole Role { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [BsonElement("sources")]
    public List<string> Sources { get; set; } = new();
}

public enum MessageRole
{
    User,
    Assistant,
    System
}
