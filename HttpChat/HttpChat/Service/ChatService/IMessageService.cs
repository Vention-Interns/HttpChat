using HttpChat.dto;

namespace HttpChat.Service.ChatService;

public interface IMessageService
{
    
    void SaveLocalMessages(IEnumerable<MessageRequestDto> messages);
    
}