using System.Security.Claims;
using HttpChat.Services.ChatService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HttpChat.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController(IChatService chatService) : ControllerBase
{
    [Authorize]
    [HttpGet("chats")]
    public async Task<IActionResult> GetUserChats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var chats = await chatService.ReceiveUserChatsAsync(userId);

        return Ok(chats);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatMessages([FromQuery] int chatId)
    {
        try
        {
            var messages = await chatService.GetChatMessagesAsync(chatId);
            return Ok(messages);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { Message = "An error occurred while fetching chat messages.", Error = ex.Message });
        }
    }
}