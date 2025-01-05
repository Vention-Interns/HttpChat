using System.ComponentModel.DataAnnotations;

namespace HttpChat.dto;

public class RegisterUserRequest
{
    [Required(ErrorMessage = "Client ID is required.")]
    [MinLength(1)]
    public string ClientId { get; set; }
    [Required(ErrorMessage = "Chat ID is required.")]
    [MinLength(1)]
    public string ChatId { get; set; }
}