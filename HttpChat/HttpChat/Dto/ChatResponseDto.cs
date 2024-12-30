using HttpChat.Model;

namespace HttpChat.dto;
public class ChatResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsGroupChat { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Participants { get; set; }
    public List<MessageResponseDto> Messages { get; set; }

    public static ChatResponseDto ToDto(Chat chat)
    {
        return new ChatResponseDto
        {
            Id = chat.Id,
            Name = chat.Name,
            IsGroupChat = chat.IsGroupChat,
            CreatedAt = chat.CreatedAt,
            Messages = chat.Messages?.Select(MessageResponseDto.ToDto).ToList() ?? new List<MessageResponseDto>()
        };
    }
}
