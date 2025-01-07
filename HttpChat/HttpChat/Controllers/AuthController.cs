using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HttpChat.Dtos;
using HttpChat.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HttpChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await authService.LoginAsync(dto);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Token = result.Token,
                    Expiration = result.Expiration
                });
            }

            return Unauthorized(result.ErrorMessage);
        }
    }
}