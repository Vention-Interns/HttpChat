using System.Collections.Concurrent;
using HttpChat.dto;
using HttpChat.Service.ChatService;
using Microsoft.AspNetCore.Mvc;

namespace HttpChat.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMessageService _chatService;

        public ChatController(IMessageService chatService)
        {
            _chatService = chatService;
        }
        
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<MessageRequestDto>> ChatMessageQueues = new();
        
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<string>>> ChatClientQueues = new();
        
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TaskCompletionSource<bool>>> WaitingClients = new();

        private const string MessageFieldsMissingError = "Message content, ClientId, and ChatId cannot be empty.";

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.ChatId))
                return BadRequest(new { Error = "Client ID and Chat ID cannot be empty." });
            
            ChatClientQueues.GetOrAdd(request.ChatId, _ => new ConcurrentDictionary<string, Queue<string>>());
            ChatMessageQueues.GetOrAdd(request.ChatId, _ => new ConcurrentQueue<MessageRequestDto>());
            WaitingClients.GetOrAdd(request.ChatId, _ => new ConcurrentDictionary<string, TaskCompletionSource<bool>>());
            
            ChatClientQueues[request.ChatId].TryAdd(request.ClientId, new Queue<string>());

            return await Task.FromResult(Ok(new { Status = "User registered successfully." }));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto message)
        {
            if (string.IsNullOrWhiteSpace(message.Content) || string.IsNullOrWhiteSpace(message.ClientId) || string.IsNullOrWhiteSpace(message.ChatId))
                return BadRequest(new { Error = MessageFieldsMissingError });

            if (!ChatMessageQueues.ContainsKey(message.ChatId))
                return BadRequest(new { Error = "Invalid Chat ID." });

            var fullMessage = $"{message.ClientId}: {message.Content}";
            
            ChatMessageQueues[message.ChatId].Enqueue(message);
            
            foreach (var clientQueue in ChatClientQueues[message.ChatId].Values)
            {
                lock (clientQueue)
                {
                    clientQueue.Enqueue(fullMessage);
                }
            }

            foreach (var waitingClient in WaitingClients[message.ChatId].Values)
            {
                waitingClient.TrySetResult(true);
            }
            
            await Task.Run(() => CheckMessages(message.ChatId));

            return Ok(new { Status = "Message sent successfully." });
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessage([FromQuery] string clientId, [FromQuery] string chatId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(chatId) || !ChatClientQueues.ContainsKey(chatId))
                return BadRequest(new { Error = "Invalid or unregistered Client ID or Chat ID." });

            var clientQueue = ChatClientQueues[chatId][clientId];
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
            WaitingClients[chatId][clientId] = tcs;

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            WaitingClients[chatId].TryRemove(clientId, out _);

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
        public async Task<IActionResult> GetMessageHistory([FromQuery] string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId) || !ChatMessageQueues.ContainsKey(chatId))
                return BadRequest(new { Error = "Invalid Chat ID." });

            var messages = await Task.FromResult(ChatMessageQueues[chatId].ToArray().Select(x => x.Content));
            return Ok(new { Messages = messages });
        }

        private void CheckMessages(string chatId)
        {
            //if (!ChatMessageQueues.ContainsKey(chatId)) return;

            //var messages = ChatMessageQueues[chatId];

            //if (messages.Count < 5) return;
            
            //_chatService.SaveLocalMessages(messages.ToList());
            //while (!messages.IsEmpty)
                //messages.TryDequeue(out _);
        }
    }
}
