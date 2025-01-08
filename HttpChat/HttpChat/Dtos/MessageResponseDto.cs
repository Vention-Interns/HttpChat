using HttpChat.Model;

namespace HttpChat.Dtos;

public class MessageResponseDto
{
    public int MessageId { get; set; }
    public string Content { get; set; }
    public string SenderId { get; set; }
    public DateTime SentAt { get; set; }
}
