using Microsoft.AspNetCore.Identity;

namespace HttpChat.Services.JwtTokenService;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(IdentityUser user);
}