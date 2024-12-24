using HttpChat.Model;

namespace HttpChat.dto;

public class MessageResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public string SenderUsername { get; set; }
    public int ChatId { get; set; }

    public static MessageResponseDto ToDto(Message message)
    {
        return new MessageResponseDto
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            SenderUsername = message.Sender?.Username,
            ChatId = message.ChatId
        };
    }
}
