using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;

namespace PersonalizedAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsightsController : ControllerBase
{
    private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<InsightsController> _logger;

    public InsightsController(IDataProcessingService dataProcessingService, ILogger<InsightsController> logger)
    {
        _dataProcessingService = dataProcessingService;
        _logger = logger;
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<GetPreferencesResponse>> GetUserPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var preferences = await _dataProcessingService.GetUserPreferencesAsync(userId);
            
            return Ok(new GetPreferencesResponse
            {
                Success = true,
                Message = "Preferences retrieved successfully",
                Preferences = preferences
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences");
            return StatusCode(500, new GetPreferencesResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("preferences")]
    public async Task<ActionResult<UpdatePreferencesResponse>> UpdateUserPreferences([FromBody] UpdatePreferencesRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataProcessingService.UpdateUserPreferencesAsync(userId, request.Preferences);
            
            return Ok(new UpdatePreferencesResponse
            {
                Success = success,
                Message = success ? "Preferences updated successfully" : "Failed to update preferences"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences");
            return StatusCode(500, new UpdatePreferencesResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("insights")]
    public async Task<ActionResult<GetInsightsResponse>> GetInsights()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var insights = await _dataProcessingService.GenerateInsightsAsync(userId);
            
            return Ok(new GetInsightsResponse
            {
                Success = true,
                Message = "Insights generated successfully",
                Insights = insights
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
            return StatusCode(500, new GetInsightsResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<ProcessDataResponse>> ProcessUserData()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataProcessingService.ProcessUserPreferencesAsync(userId);
            
            return Ok(new ProcessDataResponse
            {
                Success = success,
                Message = success ? "User data processing completed successfully" : "Failed to process user data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user data");
            return StatusCode(500, new ProcessDataResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("train-model")]
    public async Task<ActionResult<TrainModelResponse>> TrainModel()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataProcessingService.TrainModelAsync(userId);
            
            return Ok(new TrainModelResponse
            {
                Success = success,
                Message = success ? "Model training completed successfully" : "Failed to train model"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training model");
            return StatusCode(500, new TrainModelResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
    }
}

public class GetPreferencesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserPreferences? Preferences { get; set; }
}

public class UpdatePreferencesRequest
{
    public UserPreferences Preferences { get; set; } = new();
}

public class UpdatePreferencesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GetInsightsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Insight> Insights { get; set; } = new();
}

public class ProcessDataResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TrainModelResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

