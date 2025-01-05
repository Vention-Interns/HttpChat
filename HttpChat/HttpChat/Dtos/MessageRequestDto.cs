using System.ComponentModel.DataAnnotations;
using HttpChat.Model;

namespace HttpChat.dto;

public class MessageRequestDto
{
    [Required(ErrorMessage = "Client ID is required.")]
    public string ClientId { get; set; }
    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; }
    [Required(ErrorMessage = "Chat ID is required.")]
    public string ChatId { get; set; }

    public static MessageModel ToEntity(MessageRequestDto messageRequestDto)
    {
        var message = new MessageModel
        {
            Content = messageRequestDto.Content,
            // ChatId = Int32.Parse(messageRequestDto.ChatId),
            // UserId = int.TryParse(messageRequestDto.ClientId, out var userId) ? userId : 0,
            SentAt = DateTime.UtcNow
        };

        return message;
    }
}
