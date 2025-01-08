using HttpChat.Dtos;
using HttpChat.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

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
                    result.Token,
                    result.Expiration
                });
            }

            return Unauthorized(result.ErrorMessage);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await authService.RegisterAsync(dto);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    result.Token,
                    result.Expiration
                });
            }

            return Unauthorized(result.ErrorMessage);

        }
    }
}