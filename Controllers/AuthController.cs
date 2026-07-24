using Microsoft.AspNetCore.Mvc;
using SafeVault.Api.DTOs;
using SafeVault.Api.Services;

namespace SafeVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (success, message) = await _authService.RegisterAsync(dto);
        if (!success)
        {
            return BadRequest(new { error = message });
        }

        return Ok(new { message });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var authResponse = await _authService.LoginAsync(dto);
        if (authResponse == null)
        {
            // SECURITY: Never reveal whether the email or the password was incorrect!
            return Unauthorized(new { error = "Invalid email or password." });
        }

        return Ok(authResponse);
    }
}