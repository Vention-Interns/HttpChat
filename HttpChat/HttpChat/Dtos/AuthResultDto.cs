namespace HttpChat.dto;

public class AuthResultDto
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public string ErrorMessage { get; set; }
}
