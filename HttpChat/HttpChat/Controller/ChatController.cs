using System.Collections.Concurrent;
using HttpChat.dto;
using HttpChat.Service;
using Microsoft.AspNetCore.Mvc;

namespace HttpChat.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private static readonly ConcurrentQueue<MessageRequestDto> GlobalMessageQueue = new();
        private static readonly ConcurrentDictionary<string, Queue<string>> ClientMessageQueues = new();
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> WaitingClients = new();

        private const string MessageFieldsMissingError = "Message content and ClientId cannot be empty.";

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId))
                return BadRequest(new { Error = "Client ID cannot be empty." });

            ClientMessageQueues.TryAdd(request.ClientId, new Queue<string>());
            return await Task.FromResult(Ok(new { Status = "User registered successfully." }));
        }
        
        

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto message)
        {
            if (string.IsNullOrWhiteSpace(message.Content) || string.IsNullOrWhiteSpace(message.ClientId))
                return BadRequest(new { Error = MessageFieldsMissingError });

            var fullMessage = $"{message.ClientId}: {message.Content}";

            GlobalMessageQueue.Enqueue(message);

            foreach (var clientQueue in ClientMessageQueues.Values)
            {
                lock (clientQueue)
                {
                    clientQueue.Enqueue(fullMessage);
                }
            }

            foreach (var waitingClient in WaitingClients.Values)
            {
                waitingClient.TrySetResult(true);
            }

            await Task.Run(CheckMessages);

            return Ok(new { Status = "Message sent successfully." });
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessage([FromQuery] string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || !ClientMessageQueues.ContainsKey(clientId))
                return BadRequest(new { Error = "Invalid or unregistered Client ID." });

            var clientQueue = ClientMessageQueues[clientId];
            TaskCompletionSource<bool> tcs = null;

            lock (clientQueue)
            {
                if (clientQueue.Count > 0)
                {
                    var messages = clientQueue.ToArray();
                    clientQueue.Clear();
                    return Ok(new { Messages = messages });
                }
            }

            tcs = new TaskCompletionSource<bool>();
            WaitingClients[clientId] = tcs;

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            WaitingClients.TryRemove(clientId, out _);

            lock (clientQueue)
            {
                if (completedTask == tcs.Task && clientQueue.Count > 0)
                {
                    var messages = clientQueue.ToArray();
                    clientQueue.Clear();
                    return Ok(new { Messages = messages });
                }
            }

            return Ok(new { Messages = Array.Empty<string>() });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMessageHistory()
        {
            var messages = await Task.FromResult(GlobalMessageQueue.ToArray());
            return Ok(new { Messages = messages });
        }

        private void CheckMessages()
        {
            if (GlobalMessageQueue.Count < 5) return;
            _chatService.SaveLocalMessages(GlobalMessageQueue.ToList());
            GlobalMessageQueue.Clear();
        }
        
    }
}
