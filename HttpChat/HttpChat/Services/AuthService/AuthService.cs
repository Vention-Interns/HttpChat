using HttpChat.dto;
using HttpChat.Dtos;
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

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto requestDto)
    {
        var user = await _userManager.FindByEmailAsync(requestDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, requestDto.Password))
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

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto requestDto)
    {
        var user = await _userManager.FindByEmailAsync(requestDto.Email);
        if (user != null)
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "User already exists"
            };
        }

        user = new UserModel
        {
            UserName = requestDto.Username,
            Email = requestDto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, requestDto.Password);
        if (!result.Succeeded)
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        var token = _jwtTokenGenerator.GenerateJwtToken(user);
        return new AuthResultDto
        {
            IsSuccess = true,
            Token = token,
            Expiration = DateTime.UtcNow.AddDays(7) // Ensure this matches the token's expiration
        };
    }

}
