using HttpChat.dto;
using HttpChat.Model;

namespace HttpChat.Services.AuthService;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginModel model);
}