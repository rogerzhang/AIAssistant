using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Infrastructure.Configuration;

namespace PersonalizedAssistant.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<DataCollection> DataCollections => _database.GetCollection<DataCollection>("data_collections");
    public IMongoCollection<ChatSession> ChatSessions => _database.GetCollection<ChatSession>("chat_sessions");
    public IMongoCollection<GmailData> GmailData => _database.GetCollection<GmailData>("gmail_data");
    public IMongoCollection<GoogleDriveData> GoogleDriveData => _database.GetCollection<GoogleDriveData>("google_drive_data");
    public IMongoCollection<IOSContactData> IOSContactData => _database.GetCollection<IOSContactData>("ios_contact_data");
    public IMongoCollection<IOSCalendarData> IOSCalendarData => _database.GetCollection<IOSCalendarData>("ios_calendar_data");
}

