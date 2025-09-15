using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalizedAssistant.Shared.Interfaces;
using PersonalizedAssistant.Shared.Models;

namespace PersonalizedAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatAgentService _chatAgentService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatAgentService chatAgentService, ILogger<ChatController> logger)
    {
        _chatAgentService = chatAgentService;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _chatAgentService.ProcessMessageAsync(userId, request.Message, request.SessionId);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new ChatResponse
            {
                Message = "I'm sorry, I encountered an error while processing your message. Please try again.",
                SessionId = request.SessionId ?? "",
                Confidence = 0.0
            });
        }
    }

    [HttpPost("session")]
    public async Task<ActionResult<CreateSessionResponse>> CreateSession()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var session = await _chatAgentService.CreateSessionAsync(userId);
            
            return Ok(new CreateSessionResponse
            {
                Success = true,
                Message = "Session created successfully",
                Session = session
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat session");
            return StatusCode(500, new CreateSessionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<GetSessionResponse>> GetSession(string sessionId)
    {
        try
        {
            var session = await _chatAgentService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound(new GetSessionResponse
                {
                    Success = false,
                    Message = "Session not found"
                });
            }

            return Ok(new GetSessionResponse
            {
                Success = true,
                Message = "Session retrieved successfully",
                Session = session
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
            return StatusCode(500, new GetSessionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<GetUserSessionsResponse>> GetUserSessions([FromQuery] int limit = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var sessions = await _chatAgentService.GetUserSessionsAsync(userId, limit);
            
            return Ok(new GetUserSessionsResponse
            {
                Success = true,
                Message = "Sessions retrieved successfully",
                Sessions = sessions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user sessions");
            return StatusCode(500, new GetUserSessionsResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpDelete("session/{sessionId}")]
    public async Task<ActionResult<DeleteSessionResponse>> DeleteSession(string sessionId)
    {
        try
        {
            var success = await _chatAgentService.DeleteSessionAsync(sessionId);
            
            return Ok(new DeleteSessionResponse
            {
                Success = success,
                Message = success ? "Session deleted successfully" : "Failed to delete session"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session: {SessionId}", sessionId);
            return StatusCode(500, new DeleteSessionResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<GetSuggestionsResponse>> GetSuggestedQuestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var suggestions = await _chatAgentService.GetSuggestedQuestionsAsync(userId);
            
            return Ok(new GetSuggestionsResponse
            {
                Success = true,
                Message = "Suggestions retrieved successfully",
                Questions = suggestions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suggested questions");
            return StatusCode(500, new GetSuggestionsResponse
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

public class SendMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}

public class CreateSessionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ChatSession? Session { get; set; }
}

public class GetSessionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ChatSession? Session { get; set; }
}

public class GetUserSessionsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ChatSession> Sessions { get; set; } = new();
}

public class DeleteSessionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GetSuggestionsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Questions { get; set; } = new();
}
