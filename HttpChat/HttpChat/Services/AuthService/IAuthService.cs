using HttpChat.dto;
using HttpChat.Dtos;

namespace HttpChat.Services.AuthService;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto requestDto);
}