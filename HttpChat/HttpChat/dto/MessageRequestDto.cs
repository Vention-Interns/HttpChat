using HttpChat.Model;

namespace HttpChat.dto;

public class MessageRequestDto
{

    public string ClientId { get; set; }
    public string Content { get; set; }

    public static Message ToEntity(MessageRequestDto messageRequestDto)
    {
        var message = new Message
        {
            ClientId = messageRequestDto.ClientId,
            Content = messageRequestDto.Content
        };
        return message;
    }
    
}