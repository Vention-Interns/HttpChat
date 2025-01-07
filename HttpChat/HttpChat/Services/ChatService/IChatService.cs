using HttpChat.dto;
using HttpChat.dtos;

namespace HttpChat.Services.ChatService;

public interface IChatService
{
    Task<List<ChatResponseDto>> ReceiveUserChatsAsync(string clientId);
    
}