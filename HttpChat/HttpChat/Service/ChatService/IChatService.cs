using HttpChat.dto;

namespace HttpChat.Service;

public interface IChatService
{
    void SaveLocalMessages(List<MessageRequestDto> messages);
    
}