using HttpChat.Model;

namespace HttpChat.dtos;
public class ChatResponseDto
{
    public int ChatId { get; set; }
    public string ChatName { get; set; }
    public bool IsGroupChat { get; set; }
    public int NumberOfParticipants { get; set; }
}


