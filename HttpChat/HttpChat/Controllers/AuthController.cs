using HttpChat.Model;
using HttpChat.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace HttpChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await authService.LoginAsync(model);

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