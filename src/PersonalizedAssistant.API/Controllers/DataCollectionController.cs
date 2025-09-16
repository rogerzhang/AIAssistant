using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;
using PersonalizedAssistant.Shared.Enums;

namespace PersonalizedAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataCollectionController : ControllerBase
{
    private readonly IDataCollectionService _dataCollectionService;
    private readonly ILogger<DataCollectionController> _logger;

    public DataCollectionController(IDataCollectionService dataCollectionService, ILogger<DataCollectionController> logger)
    {
        _dataCollectionService = dataCollectionService;
        _logger = logger;
    }

    [HttpPost("gmail")]
    public async Task<ActionResult<DataCollectionResponse>> CollectGmailData([FromBody] CollectGmailRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataCollectionService.CollectGmailDataAsync(userId, request.AccessToken);
            
            return Ok(new DataCollectionResponse
            {
                Success = success,
                Message = success ? "Gmail data collection initiated successfully" : "Failed to collect Gmail data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Gmail data");
            return StatusCode(500, new DataCollectionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("google-drive")]
    public async Task<ActionResult<DataCollectionResponse>> CollectGoogleDriveData([FromBody] CollectGoogleDriveRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataCollectionService.CollectGoogleDriveDataAsync(userId, request.AccessToken);
            
            return Ok(new DataCollectionResponse
            {
                Success = success,
                Message = success ? "Google Drive data collection initiated successfully" : "Failed to collect Google Drive data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Google Drive data");
            return StatusCode(500, new DataCollectionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("ios-contacts")]
    public async Task<ActionResult<DataCollectionResponse>> CollectIOSContactsData([FromBody] CollectIOSContactsRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataCollectionService.CollectIOSContactsDataAsync(userId, request.Contacts);
            
            return Ok(new DataCollectionResponse
            {
                Success = success,
                Message = success ? "iOS contacts data collection completed successfully" : "Failed to collect iOS contacts data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting iOS contacts data");
            return StatusCode(500, new DataCollectionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("ios-calendar")]
    public async Task<ActionResult<DataCollectionResponse>> CollectIOSCalendarData([FromBody] CollectIOSCalendarRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _dataCollectionService.CollectIOSCalendarDataAsync(userId, request.Events);
            
            return Ok(new DataCollectionResponse
            {
                Success = success,
                Message = success ? "iOS calendar data collection completed successfully" : "Failed to collect iOS calendar data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting iOS calendar data");
            return StatusCode(500, new DataCollectionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("data")]
    public async Task<ActionResult<GetDataResponse>> GetCollectedData([FromQuery] DataSource? source = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var data = await _dataCollectionService.GetCollectedDataAsync(userId, source);
            
            return Ok(new GetDataResponse
            {
                Success = true,
                Message = "Data retrieved successfully",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving collected data");
            return StatusCode(500, new GetDataResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("data/{dataId}")]
    public async Task<ActionResult<GetDataByIdResponse>> GetDataById(string dataId)
    {
        try
        {
            var data = await _dataCollectionService.GetDataByIdAsync(dataId);
            if (data == null)
            {
                return NotFound(new GetDataByIdResponse
                {
                    Success = false,
                    Message = "Data not found"
                });
            }

            return Ok(new GetDataByIdResponse
            {
                Success = true,
                Message = "Data retrieved successfully",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data by ID: {DataId}", dataId);
            return StatusCode(500, new GetDataByIdResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpDelete("data/{dataId}")]
    public async Task<ActionResult<DataCollectionResponse>> DeleteData(string dataId)
    {
        try
        {
            var success = await _dataCollectionService.DeleteDataAsync(dataId);
            
            return Ok(new DataCollectionResponse
            {
                Success = success,
                Message = success ? "Data deleted successfully" : "Failed to delete data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data: {DataId}", dataId);
            return StatusCode(500, new DataCollectionResponse
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

public class CollectGmailRequest
{
    public string AccessToken { get; set; } = string.Empty;
}

public class CollectGoogleDriveRequest
{
    public string AccessToken { get; set; } = string.Empty;
}

public class CollectIOSContactsRequest
{
    public List<IOSContactData> Contacts { get; set; } = new();
}

public class CollectIOSCalendarRequest
{
    public List<IOSCalendarData> Events { get; set; } = new();
}

public class DataCollectionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GetDataResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<DataCollection> Data { get; set; } = new();
}

public class GetDataByIdResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DataCollection? Data { get; set; }
}

