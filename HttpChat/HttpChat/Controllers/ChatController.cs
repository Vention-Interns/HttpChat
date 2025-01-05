using HttpChat.dto;
using HttpChat.Service.ChatService;
using Microsoft.AspNetCore.Mvc;

namespace HttpChat.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMessageService _messageService;
        public ChatController(IMessageService messageService)
        {
            _messageService = messageService;
            _messageService.StartInactiveClientCleanupTask();
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.ChatId)) { 
                return BadRequest(new { Error = "Client ID and Chat ID cannot be empty." });
            }

            _messageService.RegisterUser(request);
            
            return await Task.FromResult(Ok(new { Status = "User registered successfully." }));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto message)
        {
            if (string.IsNullOrWhiteSpace(message.Content) || 
                string.IsNullOrWhiteSpace(message.ClientId) ||
                string.IsNullOrWhiteSpace(message.ChatId)) 
            { 
                return BadRequest(new { Error = "Message content, ClientId, and ChatId cannot be empty." });
            }

            var chatId = message.ChatId;

            if (!await _messageService.IsChatIdValid(chatId)) 
            { 
                return BadRequest(new { Error = "Invalid Chat ID." });
            }

            _messageService.SendMessage(message);

            return Ok(new { Status = "Message sent successfully." });
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessage([FromQuery] string clientId, [FromQuery] string chatId)
        {
            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(chatId) ||
                !_messageService.IsClientChatSetted(chatId, clientId)) 
            { 
                return BadRequest(new { Error = "Invalid or unregistered Client ID or Chat ID." });
            }

            var messages = await _messageService.ReceiveMessageAsync(chatId, clientId);

            if (messages == null || messages.Length == 0)
            {
                return Ok(new { Messages = Array.Empty<string>() });
            }

            return Ok(new { Messages = messages });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMessageHistory([FromQuery] string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                return BadRequest(new { Error = "Invalid Chat ID." });
            }

            var messages = await _messageService.GetMessageHistoryAsync(chatId);

            if (messages == null)
            {
                return BadRequest(new { Error = "Invalid or non-existent Chat ID." });
            }

            return Ok(new { Messages = messages });
        }       
    }
}