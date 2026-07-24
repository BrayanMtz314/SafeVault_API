using SafeVault.Api.DTOs;

namespace SafeVault.Api.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}

