using HttpChat.dto;

namespace HttpChat.Service.ChatService;

public interface IMessageService
{
    
    void SaveLocalMessages(IEnumerable<MessageRequestDto> messages);
    void RegisterUser(RegisterUserRequest request);
    void SendMessage(MessageRequestDto message);
    Task<bool> IsChatIdValid(string chatId);
    bool IsClientChatSetted(String chatId, String clientId);

    Task<string[]> ReceiveMessageAsync(String chatId, String clientId);
    Task<IEnumerable<string>?> GetMessageHistoryAsync(string chatId);
    void StartInactiveClientCleanupTask();

}