using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthenticateRequest request)
    {
        try
        {
            var user = await _authService.AuthenticateUserAsync(request.GoogleToken);
            
            if (user == null)
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid Google token or user not found"
                });
            }

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Authentication successful",
                User = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "Internal server error during authentication"
            });
        }
    }

    [HttpPost("permissions/grant")]
    [Authorize]
    public async Task<ActionResult<PermissionResponse>> GrantPermission([FromBody] GrantPermissionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var permission = new DataPermission
            {
                Source = request.Source,
                GrantedAt = DateTime.UtcNow,
                IsActive = true,
                Scope = request.Scope
            };

            var success = await _authService.GrantDataPermissionAsync(userId, permission);
            
            return Ok(new PermissionResponse
            {
                Success = success,
                Message = success ? "Permission granted successfully" : "Failed to grant permission"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permission");
            return StatusCode(500, new PermissionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("permissions/revoke")]
    [Authorize]
    public async Task<ActionResult<PermissionResponse>> RevokePermission([FromBody] RevokePermissionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _authService.RevokeDataPermissionAsync(userId, request.Source);
            
            return Ok(new PermissionResponse
            {
                Success = success,
                Message = success ? "Permission revoked successfully" : "Failed to revoke permission"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission");
            return StatusCode(500, new PermissionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("permissions")]
    [Authorize]
    public async Task<ActionResult<PermissionsResponse>> GetPermissions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var permissions = await _authService.GetUserPermissionsAsync(userId);
            
            return Ok(new PermissionsResponse
            {
                Success = true,
                Message = "Permissions retrieved successfully",
                Permissions = permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, new PermissionsResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<User>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
    }
}

public class AuthenticateRequest
{
    public string GoogleToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
}

public class GrantPermissionRequest
{
    public DataSource Source { get; set; }
    public List<string> Scope { get; set; } = new();
}

public class RevokePermissionRequest
{
    public DataSource Source { get; set; }
}

public class PermissionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PermissionsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<DataPermission> Permissions { get; set; } = new();
}
