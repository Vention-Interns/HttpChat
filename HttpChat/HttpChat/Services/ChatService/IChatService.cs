using HttpChat.dto;
using HttpChat.dtos;
using HttpChat.Dtos;

namespace HttpChat.Services.ChatService;

public interface IChatService
{
    Task<List<ChatResponseDto>> ReceiveUserChatsAsync(string clientId);
    
    Task<List<MessageResponseDto>> GetChatMessagesAsync(int chatId);
    
}