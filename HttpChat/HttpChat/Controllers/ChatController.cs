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
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            _messageService.RegisterUser(request);

            return await Task.FromResult(Ok(new { Status = "User registered successfully." }));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (!await _messageService.IsChatIdValid(message.ChatId))
            {
                return BadRequest(new { Error = "Invalid Chat ID." });
            }

            _messageService.SendMessage(message);

            return Ok(new { Status = "Message sent successfully." });
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessage([FromQuery] string clientId, [FromQuery] string chatId)
        {
            var validationError = ValidateReceiveMessage(clientId, chatId);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(new { Error = validationError });
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

        private string ValidateReceiveMessage(string clientId, string chatId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return "Client ID cannot be empty.";
            }

            if (string.IsNullOrWhiteSpace(chatId))
            {
                return "Chat ID cannot be empty.";
            }

            if (!_messageService.IsClientChatSetted(chatId, clientId))
            {
                return "Invalid or unregistered Client ID or Chat ID.";
            }

            return string.Empty;
        }
    }
}