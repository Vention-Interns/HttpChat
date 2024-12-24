using HttpChat.Model;

namespace HttpChat.dto;

using System;
using System.Collections.Generic;

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
            Participants = chat.Participants?.Select(p => p.Username).ToList() ?? new List<string>(),
            Messages = chat.Messages?.Select(MessageResponseDto.ToDto).ToList() ?? new List<MessageResponseDto>()
        };
    }
}
