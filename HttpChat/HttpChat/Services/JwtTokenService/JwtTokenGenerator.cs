using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HttpChat.Services.JwtTokenService;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _jwtSecretKey;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _jwtSecretKey = configuration["AppSettings:JwtSecretKey"]
                        ?? throw new ArgumentNullException(nameof(configuration), "JwtSecretKey is not configured.");
    }

    public string GenerateJwtToken(IdentityUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = "http://localhost:5000", 
            Audience = "http://localhost:5000",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}
