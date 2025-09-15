using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalizedAssistant.Infrastructure.Configuration;
using PersonalizedAssistant.Infrastructure.Data;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace PersonalizedAssistant.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _context;
    private readonly AppSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(MongoDbContext context, IOptions<AppSettings> settings, ILogger<AuthService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<User?> AuthenticateUserAsync(string googleToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
            
            if (payload == null)
            {
                _logger.LogWarning("Invalid Google token provided");
                return null;
            }

            var user = await GetUserByGoogleIdAsync(payload.Subject);
            if (user != null)
            {
                await UpdateUserLastLoginAsync(user.Id);
                return user;
            }

            // Create new user if not exists
            return await CreateUserAsync(payload.Subject, payload.Email, payload.Name, payload.Picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user with Google token");
            return null;
        }
    }

    public async Task<User> CreateUserAsync(string googleId, string email, string name, string? profilePicture = null)
    {
        var user = new User
        {
            GoogleId = googleId,
            Email = email,
            Name = name,
            ProfilePicture = profilePicture,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Permissions = new List<DataPermission>()
        };

        await _context.Users.InsertOneAsync(user);
        _logger.LogInformation("Created new user: {UserId}", user.Id);
        
        return user;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        return await _context.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByGoogleIdAsync(string googleId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.GoogleId, googleId);
        return await _context.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateUserLastLoginAsync(string userId)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            
            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> GrantDataPermissionAsync(string userId, DataPermission permission)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Push(u => u.Permissions, permission);
            
            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting data permission for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> RevokeDataPermissionAsync(string userId, DataSource source)
    {
        try
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.Permissions, p => p.Source == source)
            );
            
            var update = Builders<User>.Update.Set("permissions.$.isActive", false);
            
            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking data permission for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<DataPermission>> GetUserPermissionsAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        return user?.Permissions.Where(p => p.IsActive).ToList() ?? new List<DataPermission>();
    }
}
