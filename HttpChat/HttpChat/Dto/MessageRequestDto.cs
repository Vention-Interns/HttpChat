using HttpChat.Model;

namespace HttpChat.dto;

public class MessageRequestDto
{
    public string ClientId { get; set; }
    public string Content { get; set; }
    public string ChatId { get; set; }

    public static Message ToEntity(MessageRequestDto messageRequestDto)
    {
        var message = new Message
        {
            Content = messageRequestDto.Content,
            ChatId = Int32.Parse(messageRequestDto.ChatId),
            UserId = int.TryParse(messageRequestDto.ClientId, out var userId) ? userId : 0,
            SentAt = DateTime.UtcNow
        };
        return message;
    }
}
