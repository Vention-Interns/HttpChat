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
        private readonly IMessageService _messageService;


        private static readonly ConcurrentDictionary<string, ConcurrentQueue<MessageRequestDto>> _chatMessageQueues =
            new();

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<string>>>
            _chatClientQueues = new();

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TaskCompletionSource<bool>>>
            _waitingClients = new();

        private static readonly ConcurrentDictionary<string, DateTime> _clientLastActive = new();

        private readonly TimeSpan _inactiveThreshold = TimeSpan.FromSeconds(10);
        public ChatController(IMessageService messageService)
        {
            _messageService = messageService;
            StartInactiveClientCleanupTask();
        }

        private void StartInactiveClientCleanupTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    CleanupInactiveClientsAndChats();
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            });
        }

        private void CleanupInactiveClientsAndChats()
        {
            var now = DateTime.UtcNow;

            foreach (var chatId in _chatClientQueues.Keys.ToList())
            {
                if (_chatClientQueues.TryGetValue(chatId, out var clientQueues))
                {
                    foreach (var clientId in clientQueues.Keys.ToList())
                    {
                        if (_clientLastActive.TryGetValue(clientId, out var lastActive) &&
                            now - lastActive > _inactiveThreshold)
                        {
                            Console.WriteLine("Removing User " + clientId);
                            clientQueues.TryRemove(clientId, out _);
                            _clientLastActive.TryRemove(clientId, out _);
                            Console.WriteLine($"Removed inactive client {clientId} from chat {chatId}.");
                        }
                    }

                    if (clientQueues.IsEmpty)
                    {
                        _chatClientQueues.TryRemove(chatId, out _);
                        _chatMessageQueues.TryRemove(chatId, out _);
                        _waitingClients.TryRemove(chatId, out _);
                        Console.WriteLine($"Removed empty chat {chatId}.");
                    }
                }
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.ChatId)) { 
                return BadRequest(new { Error = "Client ID and Chat ID cannot be empty." });
            }

            _chatClientQueues.GetOrAdd(request.ChatId, _ => new ConcurrentDictionary<string, Queue<string>>());
            _chatMessageQueues.GetOrAdd(request.ChatId, _ => new ConcurrentQueue<MessageRequestDto>());
            _waitingClients.GetOrAdd(request.ChatId,
                _ => new ConcurrentDictionary<string, TaskCompletionSource<bool>>());

            _chatClientQueues[request.ChatId].TryAdd(request.ClientId, new Queue<string>());
            _clientLastActive[request.ClientId] = DateTime.UtcNow;

            return await Task.FromResult(Ok(new { Status = "User registered successfully." }));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto message)
        {
            if (string.IsNullOrWhiteSpace(message.Content) || string.IsNullOrWhiteSpace(message.ClientId) ||
                string.IsNullOrWhiteSpace(message.ChatId)) { 
                return BadRequest(new { Error = "Message content, ClientId, and ChatId cannot be empty." });
            }

            if (!_chatMessageQueues.ContainsKey(message.ChatId)) { 
                return BadRequest(new { Error = "Invalid Chat ID." });
            }

            var fullMessage = $"{message.ClientId}: {message.Content}";

            _chatMessageQueues[message.ChatId].Enqueue(message);

            foreach (var clientQueue in _chatClientQueues[message.ChatId].Values)
            {
                lock (clientQueue)
                {
                    clientQueue.Enqueue(fullMessage);
                }
            }

            foreach (var waitingClient in _waitingClients[message.ChatId].Values)
            {
                waitingClient.TrySetResult(true);
            }

            return Ok(new { Status = "Message sent successfully." });
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessage([FromQuery] string clientId, [FromQuery] string chatId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(chatId) ||
                !_chatClientQueues.ContainsKey(chatId)) { 
                return BadRequest(new { Error = "Invalid or unregistered Client ID or Chat ID." });
            }

            if (!_chatClientQueues[chatId].ContainsKey(clientId)) { 
                return BadRequest(new { Error = "Client is not registered in the specified chat." });
            }

            _clientLastActive[clientId] = DateTime.UtcNow;

            var clientQueue = _chatClientQueues[chatId][clientId];
            TaskCompletionSource<bool> tcs = null;

            lock (clientQueue)
            {
                if (clientQueue.Count > 0)
                {
                    var messages = clientQueue.ToArray();
                    clientQueue.Clear();

                    CheckAndSaveMessages(chatId); // Save messages only if all clients have received them

                    return Ok(new { Messages = messages });
                }
            }

            tcs = new TaskCompletionSource<bool>();
            _waitingClients[chatId][clientId] = tcs;

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            _waitingClients[chatId].TryRemove(clientId, out _);

            lock (clientQueue)
            {
                if (completedTask == tcs.Task && clientQueue.Count > 0)
                {
                    var messages = clientQueue.ToArray();
                    clientQueue.Clear();

                    CheckAndSaveMessages(chatId); // Save messages only if all clients have received them

                    return Ok(new { Messages = messages });
                }
            }

            return Ok(new { Messages = Array.Empty<string>() });
        }

        private void CheckAndSaveMessages(string chatId)
        {
            if (_chatClientQueues.TryGetValue(chatId, out var clientQueues))
            {
                foreach (var clientQueue in clientQueues.Values)
                {
                    lock (clientQueue)
                    {
                        if (clientQueue.Count > 0)
                        {
                            return;
                        }
                    }
                }

                if (_chatMessageQueues.TryGetValue(chatId, out var messageQueue))
                {
                    SaveLocalMessages(chatId, messageQueue.Select(m => $"{m.ClientId}: {m.Content}"));

                    while (messageQueue.TryDequeue(out _))
                    {
                    }
                }
            }
        }

        private void SaveLocalMessages(string chatId, IEnumerable<string> messages)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save messages for Chat ID {chatId}: {ex.Message}");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMessageHistory([FromQuery] string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId) || !_chatMessageQueues.ContainsKey(chatId)) { 
                return BadRequest(new { Error = "Invalid Chat ID." });
            }

            var messages = await Task.FromResult(_chatMessageQueues[chatId].ToArray().Select(x => x.Content));

            return Ok(new { Messages = messages });
        }
    }
}