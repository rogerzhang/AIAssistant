using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalizedAssistant.Infrastructure.Configuration;
using PersonalizedAssistant.Infrastructure.Data;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace PersonalizedAssistant.Infrastructure.Services;

public class ChatAgentService : IChatAgentService
{
    private readonly MongoDbContext _context;
    private readonly AppSettings _settings;
    private readonly ILogger<ChatAgentService> _logger;
    private readonly IDataProcessingService _dataProcessingService;

    public ChatAgentService(
        MongoDbContext context, 
        IOptions<AppSettings> settings, 
        ILogger<ChatAgentService> logger,
        IDataProcessingService dataProcessingService)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
        _dataProcessingService = dataProcessingService;
    }

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message, string? sessionId = null)
    {
        try
        {
            var session = string.IsNullOrEmpty(sessionId) 
                ? await CreateSessionAsync(userId)
                : await GetSessionAsync(sessionId);

            if (session == null)
            {
                return new ChatResponse
                {
                    Message = "I'm sorry, I couldn't create or retrieve your chat session. Please try again.",
                    SessionId = sessionId ?? "",
                    Confidence = 0.0
                };
            }

            // Add user message to session
            var userMessage = new ChatMessage
            {
                Content = message,
                Role = MessageRole.User,
                Timestamp = DateTime.UtcNow
            };
            session.Messages.Add(userMessage);

            // Process the message and generate response
            var response = await GenerateResponseAsync(userId, message, session);
            
            // Add assistant response to session
            var assistantMessage = new ChatMessage
            {
                Content = response.Message,
                Role = MessageRole.Assistant,
                Timestamp = DateTime.UtcNow,
                Sources = response.Sources,
                Metadata = response.Metadata
            };
            session.Messages.Add(assistantMessage);

            // Update session
            session.LastActivityAt = DateTime.UtcNow;
            await UpdateSessionAsync(session);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for user: {UserId}", userId);
            return new ChatResponse
            {
                Message = "I'm sorry, I encountered an error while processing your message. Please try again.",
                SessionId = sessionId ?? "",
                Confidence = 0.0
            };
        }
    }

    public async Task<ChatSession> CreateSessionAsync(string userId)
    {
        var session = new ChatSession
        {
            UserId = userId,
            SessionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            Messages = new List<ChatMessage>(),
            Context = new Dictionary<string, object>()
        };

        await _context.ChatSessions.InsertOneAsync(session);
        _logger.LogInformation("Created new chat session: {SessionId} for user: {UserId}", session.SessionId, userId);
        
        return session;
    }

    public async Task<ChatSession?> GetSessionAsync(string sessionId)
    {
        var filter = Builders<ChatSession>.Filter.Eq(s => s.SessionId, sessionId);
        return await _context.ChatSessions.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateSessionAsync(ChatSession session)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.Eq(s => s.SessionId, session.SessionId);
            var result = await _context.ChatSessions.ReplaceOneAsync(filter, session);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session: {SessionId}", session.SessionId);
            return false;
        }
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId, int limit = 10)
    {
        var filter = Builders<ChatSession>.Filter.Eq(s => s.UserId, userId);
        return await _context.ChatSessions
            .Find(filter)
            .SortByDescending(s => s.LastActivityAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.Eq(s => s.SessionId, sessionId);
            var result = await _context.ChatSessions.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session: {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(string userId)
    {
        var suggestions = new List<string>
        {
            "Who am I?",
            "What do I like to eat?",
            "What are my passions?",
            "What are my doctors?",
            "Who are my friends?",
            "What tasks should I do?",
            "What are my recent interests?",
            "Who do I work with?",
            "What are my upcoming events?",
            "What files have I been working on?"
        };

        return suggestions;
    }

    private async Task<ChatResponse> GenerateResponseAsync(string userId, string message, ChatSession session)
    {
        var response = new ChatResponse
        {
            SessionId = session.SessionId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Get user preferences for context
            var preferences = await _dataProcessingService.GetUserPreferencesAsync(userId);
            
            // Analyze the message to determine intent
            var intent = AnalyzeIntent(message);
            
            switch (intent)
            {
                case "who_am_i":
                    response = await HandleWhoAmIQuery(userId, preferences, response);
                    break;
                case "interests":
                    response = await HandleInterestsQuery(userId, preferences, response);
                    break;
                case "relationships":
                    response = await HandleRelationshipsQuery(userId, preferences, response);
                    break;
                case "tasks":
                    response = await HandleTasksQuery(userId, preferences, response);
                    break;
                case "health":
                    response = await HandleHealthQuery(userId, preferences, response);
                    break;
                case "work":
                    response = await HandleWorkQuery(userId, preferences, response);
                    break;
                case "files":
                    response = await HandleFilesQuery(userId, preferences, response);
                    break;
                case "calendar":
                    response = await HandleCalendarQuery(userId, preferences, response);
                    break;
                default:
                    response = await HandleGeneralQuery(userId, message, preferences, response);
                    break;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response for user: {UserId}", userId);
            response.Message = "I'm sorry, I couldn't process your request at the moment. Please try again.";
            response.Confidence = 0.0;
            return response;
        }
    }

    private string AnalyzeIntent(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("who am i") || lowerMessage.Contains("tell me about myself"))
            return "who_am_i";
        if (lowerMessage.Contains("like") || lowerMessage.Contains("interest") || lowerMessage.Contains("passion"))
            return "interests";
        if (lowerMessage.Contains("friend") || lowerMessage.Contains("relationship") || lowerMessage.Contains("know"))
            return "relationships";
        if (lowerMessage.Contains("task") || lowerMessage.Contains("todo") || lowerMessage.Contains("do"))
            return "tasks";
        if (lowerMessage.Contains("doctor") || lowerMessage.Contains("health") || lowerMessage.Contains("medical"))
            return "health";
        if (lowerMessage.Contains("work") || lowerMessage.Contains("colleague") || lowerMessage.Contains("company"))
            return "work";
        if (lowerMessage.Contains("file") || lowerMessage.Contains("document") || lowerMessage.Contains("drive"))
            return "files";
        if (lowerMessage.Contains("calendar") || lowerMessage.Contains("event") || lowerMessage.Contains("meeting"))
            return "calendar";

        return "general";
    }

    private async Task<ChatResponse> HandleWhoAmIQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        
        if (user == null)
        {
            response.Message = "I don't have information about you yet. Please make sure you're logged in correctly.";
            response.Confidence = 0.0;
            return response;
        }

        var message = $"Hello {user.Name}! Based on your data, I can see that:\n\n";
        
        if (preferences?.Interests.Any() == true)
        {
            message += $"• You're interested in: {string.Join(", ", preferences.Interests.Take(5))}\n";
        }
        
        if (preferences?.Relationships.Any() == true)
        {
            message += $"• You have {preferences.Relationships.Count} contacts in your network\n";
        }
        
        if (preferences?.Tasks.Any() == true)
        {
            var pendingTasks = preferences.Tasks.Count(t => !t.IsCompleted);
            message += $"• You have {pendingTasks} pending tasks\n";
        }

        response.Message = message;
        response.Confidence = 0.9;
        response.Sources.Add("User Profile");
        response.Sources.Add("Processed Data");
        
        return response;
    }

    private async Task<ChatResponse> HandleInterestsQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        if (preferences?.Interests.Any() != true)
        {
            response.Message = "I don't have enough data to identify your interests yet. Try connecting more data sources or give me some time to analyze your information.";
            response.Confidence = 0.0;
            return response;
        }

        var message = $"Based on your data, here are your interests:\n\n";
        foreach (var interest in preferences.Interests.Take(10))
        {
            message += $"• {interest}\n";
        }

        response.Message = message;
        response.Confidence = 0.8;
        response.Sources.Add("Email Analysis");
        response.Sources.Add("File Analysis");
        
        return response;
    }

    private async Task<ChatResponse> HandleRelationshipsQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        if (preferences?.Relationships.Any() != true)
        {
            response.Message = "I don't have information about your relationships yet. Make sure to sync your contacts.";
            response.Confidence = 0.0;
            return response;
        }

        var message = "Here are the people in your network:\n\n";
        
        var doctors = preferences.Relationships.Where(r => r.Type == "Doctor").ToList();
        if (doctors.Any())
        {
            message += "**Doctors:**\n";
            foreach (var doctor in doctors)
            {
                message += $"• {doctor.Name} ({doctor.ContactInfo?.Organization})\n";
            }
            message += "\n";
        }

        var colleagues = preferences.Relationships.Where(r => r.Type == "Colleague").ToList();
        if (colleagues.Any())
        {
            message += "**Colleagues:**\n";
            foreach (var colleague in colleagues.Take(5))
            {
                message += $"• {colleague.Name} ({colleague.ContactInfo?.Organization})\n";
            }
            message += "\n";
        }

        var friends = preferences.Relationships.Where(r => r.Type == "Friend").ToList();
        if (friends.Any())
        {
            message += "**Friends:**\n";
            foreach (var friend in friends.Take(5))
            {
                message += $"• {friend.Name}\n";
            }
        }

        response.Message = message;
        response.Confidence = 0.9;
        response.Sources.Add("Contacts");
        response.Sources.Add("Email Analysis");
        
        return response;
    }

    private async Task<ChatResponse> HandleTasksQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        if (preferences?.Tasks.Any() != true)
        {
            response.Message = "I don't have any tasks identified for you yet. Try syncing your calendar or give me more time to analyze your data.";
            response.Confidence = 0.0;
            return response;
        }

        var pendingTasks = preferences.Tasks.Where(t => !t.IsCompleted).ToList();
        var completedTasks = preferences.Tasks.Where(t => t.IsCompleted).ToList();

        var message = $"**Task Summary:**\n\n";
        message += $"• Pending: {pendingTasks.Count}\n";
        message += $"• Completed: {completedTasks.Count}\n\n";

        if (pendingTasks.Any())
        {
            message += "**Pending Tasks:**\n";
            foreach (var task in pendingTasks.Take(5))
            {
                var dueDate = task.DueDate?.ToString("MMM dd") ?? "No due date";
                message += $"• {task.Title} (Due: {dueDate})\n";
            }
        }

        response.Message = message;
        response.Confidence = 0.9;
        response.Sources.Add("Calendar Events");
        response.Sources.Add("Email Analysis");
        
        return response;
    }

    private async Task<ChatResponse> HandleHealthQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        if (preferences?.HealthInfo == null)
        {
            response.Message = "I don't have health information for you yet. Make sure to sync your contacts to identify doctors and medical contacts.";
            response.Confidence = 0.0;
            return response;
        }

        var message = "**Health Information:**\n\n";
        
        if (preferences.HealthInfo.Doctors.Any())
        {
            message += "**Your Doctors:**\n";
            foreach (var doctor in preferences.HealthInfo.Doctors)
            {
                message += $"• Dr. {doctor.Name} - {doctor.Specialty}\n";
                if (doctor.LastVisit.HasValue)
                {
                    message += $"  Last visit: {doctor.LastVisit.Value:MMM dd, yyyy}\n";
                }
            }
            message += "\n";
        }

        if (preferences.HealthInfo.Medications.Any())
        {
            message += "**Medications:**\n";
            foreach (var medication in preferences.HealthInfo.Medications)
            {
                message += $"• {medication}\n";
            }
            message += "\n";
        }

        if (preferences.HealthInfo.Allergies.Any())
        {
            message += "**Allergies:**\n";
            foreach (var allergy in preferences.HealthInfo.Allergies)
            {
                message += $"• {allergy}\n";
            }
        }

        response.Message = message;
        response.Confidence = 0.9;
        response.Sources.Add("Contacts");
        response.Sources.Add("Email Analysis");
        
        return response;
    }

    private async Task<ChatResponse> HandleWorkQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        if (preferences?.WorkInfo == null)
        {
            response.Message = "I don't have work information for you yet. Try syncing your contacts and calendar to get work-related insights.";
            response.Confidence = 0.0;
            return response;
        }

        var message = "**Work Information:**\n\n";
        
        if (!string.IsNullOrEmpty(preferences.WorkInfo.Company))
        {
            message += $"**Company:** {preferences.WorkInfo.Company}\n";
        }
        
        if (!string.IsNullOrEmpty(preferences.WorkInfo.Position))
        {
            message += $"**Position:** {preferences.WorkInfo.Position}\n";
        }
        
        if (preferences.WorkInfo.Colleagues.Any())
        {
            message += $"\n**Colleagues ({preferences.WorkInfo.Colleagues.Count}):**\n";
            foreach (var colleague in preferences.WorkInfo.Colleagues.Take(5))
            {
                message += $"• {colleague.Name}\n";
            }
        }
        
        if (preferences.WorkInfo.Projects.Any())
        {
            message += $"\n**Projects:**\n";
            foreach (var project in preferences.WorkInfo.Projects)
            {
                message += $"• {project}\n";
            }
        }

        response.Message = message;
        response.Confidence = 0.8;
        response.Sources.Add("Contacts");
        response.Sources.Add("Calendar Events");
        response.Sources.Add("Email Analysis");
        
        return response;
    }

    private async Task<ChatResponse> HandleFilesQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        var recentFiles = await _context.GoogleDriveData
            .Find(d => d.UserId == userId)
            .SortByDescending(d => d.LastModified)
            .Limit(10)
            .ToListAsync();

        if (!recentFiles.Any())
        {
            response.Message = "I don't have information about your files yet. Make sure to connect your Google Drive.";
            response.Confidence = 0.0;
            return response;
        }

        var message = "**Recent Files:**\n\n";
        foreach (var file in recentFiles)
        {
            var modifiedDate = file.LastModified.ToString("MMM dd, yyyy");
            var fileSize = FormatFileSize(file.FileSize);
            message += $"• {file.FileName} ({file.FileType}) - {modifiedDate} ({fileSize})\n";
        }

        response.Message = message;
        response.Confidence = 0.9;
        response.Sources.Add("Google Drive");
        
        return response;
    }

    private async Task<ChatResponse> HandleCalendarQuery(string userId, UserPreferences? preferences, ChatResponse response)
    {
        var upcomingEvents = await _context.IOSCalendarData
            .Find(d => d.UserId == userId && d.StartTime > DateTime.UtcNow)
            .SortBy(d => d.StartTime)
            .Limit(10)
            .ToListAsync();

        if (!upcomingEvents.Any())
        {
            response.Message = "I don't have upcoming events for you. Make sure to sync your calendar.";
            response.Confidence = 0.0;
            return response;
        }

        var message = "**Upcoming Events:**\n\n";
        foreach (var eventItem in upcomingEvents)
        {
            var startTime = eventItem.StartTime.ToString("MMM dd, yyyy HH:mm");
            var location = string.IsNullOrEmpty(eventItem.Location) ? "" : $" at {eventItem.Location}";
            message += $"• {eventItem.Title} - {startTime}{location}\n";
        }

        response.Message = message;
        response.Confidence = 0.9;
        response.Sources.Add("Calendar");
        
        return response;
    }

    private async Task<ChatResponse> HandleGeneralQuery(string userId, string message, UserPreferences? preferences, ChatResponse response)
    {
        response.Message = "I understand you're asking about something, but I need more specific information. Try asking about your interests, relationships, tasks, or work. You can also ask 'Who am I?' to get a general overview.";
        response.Confidence = 0.3;
        response.Sources.Add("General Knowledge");
        
        return response;
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
