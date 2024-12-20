using HttpChat.Model;

namespace HttpChat.dto;

public class MessageResponseDto
{
    public Guid MessageId { get; set; }
    public string ClientId { get; set; }
    public string Content { get; set; }

    public static MessageResponseDto ToEntity(Message message)
    {
       return new MessageResponseDto()
        {
            MessageId = message.Id,
            ClientId = message.ClientId,
            Content = message.Content
        };
       
    }
}