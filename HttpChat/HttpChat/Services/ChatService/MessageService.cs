using System.Collections.Concurrent;
using HttpChat.dto;
using HttpChat.persistence;

namespace HttpChat.Service.ChatService;

public class MessageService : IMessageService
{
    private readonly ChatDbContext _appDbContext;

    private static readonly ConcurrentDictionary<string, ConcurrentQueue<MessageRequestDto>> _chatMessageQueues =
            new();

    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<string>>>
        _chatClientQueues = new();

    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TaskCompletionSource<bool>>>
        _waitingClients = new();

    private static readonly ConcurrentDictionary<string, DateTime> _clientLastActive = new();
    private readonly TimeSpan _inactiveThreshold = TimeSpan.FromSeconds(10);
    public MessageService(ChatDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void RegisterUser(RegisterUserRequest request) {
        _chatClientQueues.GetOrAdd(request.ChatId, _ => new ConcurrentDictionary<string, Queue<string>>());
        _chatMessageQueues.GetOrAdd(request.ChatId, _ => new ConcurrentQueue<MessageRequestDto>());
        _waitingClients.GetOrAdd(request.ChatId,
            _ => new ConcurrentDictionary<string, TaskCompletionSource<bool>>());
        _chatClientQueues[request.ChatId].TryAdd(request.ClientId, new Queue<string>());
        _clientLastActive[request.ClientId] = DateTime.UtcNow;
    }

    public Task<bool> IsChatIdValid(string chatId)
    {
        return Task.FromResult(_chatMessageQueues.ContainsKey(chatId));
    }

    public void SendMessage(MessageRequestDto message) {
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
    }

    public void SaveLocalMessages(IEnumerable<MessageRequestDto> messages)
    {
        var messageEntities = messages.Select(MessageRequestDto.ToEntity).ToList();

        Console.WriteLine("Recived Messages " + messageEntities);

        _appDbContext.Messages.AddRange(messageEntities);

        _appDbContext.SaveChanges();
    }

    public bool IsClientChatSetted(string chatId, string clientId)
    {
        return _chatClientQueues.ContainsKey(chatId) && _chatClientQueues[chatId].ContainsKey(clientId);
    }

    public async Task<string[]> ReceiveMessageAsync(string chatId, string clientId)
    {
        _clientLastActive[clientId] = DateTime.UtcNow;

        var clientQueue = _chatClientQueues[chatId][clientId];
        TaskCompletionSource<bool>? tcs = null;

        lock (clientQueue)
        {
            if (clientQueue.Count > 0)
            {
                var messages = clientQueue.ToArray();
                clientQueue.Clear();
                CheckAndSaveMessages(chatId);

                return messages;
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
                CheckAndSaveMessages(chatId);

                return messages;
            }
        }

        return Array.Empty<string>();
    }

    public async Task<IEnumerable<string>?> GetMessageHistoryAsync(string chatId)
    {
        if (!_chatMessageQueues.ContainsKey(chatId))
        {
            return null;
        }

        var messages = await Task.FromResult(
            _chatMessageQueues[chatId].ToArray().Select(x => x.Content)
        );

        return messages;
    }

    public void StartInactiveClientCleanupTask() {
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

}