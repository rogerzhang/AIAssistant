using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.Shared.Interfaces;

public interface IAuthService
{
    Task<User?> AuthenticateUserAsync(string googleToken);
    Task<User> CreateUserAsync(string googleId, string email, string name, string? profilePicture = null);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByGoogleIdAsync(string googleId);
    Task<bool> UpdateUserLastLoginAsync(string userId);
    Task<bool> GrantDataPermissionAsync(string userId, DataPermission permission);
    Task<bool> RevokeDataPermissionAsync(string userId, DataSource source);
    Task<List<DataPermission>> GetUserPermissionsAsync(string userId);
}
