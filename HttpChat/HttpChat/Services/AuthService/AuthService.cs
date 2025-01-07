using HttpChat.dto;
using HttpChat.Model;
using HttpChat.Services.JwtTokenService;
using Microsoft.AspNetCore.Identity;

namespace HttpChat.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly UserManager<UserModel> _userManager;
    
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(UserManager<UserModel> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return new AuthResultDto()
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email or password"
            };
        var token = _jwtTokenGenerator.GenerateJwtToken(user);
        return new AuthResultDto()
        {
            IsSuccess = true,
            Token = token,
            Expiration = DateTime.UtcNow.AddDays(7)
        };

    }
}
