using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Api.DTOs;

namespace SafeVault.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
    {
        // 1. Ensure the requested role exists in the database
        if (!await _roleManager.RoleExistsAsync(dto.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(dto.Role));
        }

        // 2. Check if a user with this email already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return (false, "A user with this email address already exists.");
        }

        // 3. Create the new user (Identity hashes the password automatically!)
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Registration failed: {errors}");
        }

        // 4. Assign the user to their designated role (User or Admin)
        await _userManager.AddToRoleAsync(user, dto.Role);

        return (true, "User registered successfully.");
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        // 1. Find user by email
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        // 2. Validate password (and respect lockout policies from Part 1)
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid) return null;

        // 3. Retrieve user roles
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "User";

        // 4. Generate JWT
        return GenerateJwtToken(user, userRole);
    }

    private AuthResponseDto GenerateJwtToken(IdentityUser user, string role)
    {
        var secretKey = _configuration["Jwt:Key"] ?? "SafeVault_Super_Secret_Development_Key_2026!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        // Embed identity claims directly inside the token payload
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique Token ID
        };

        var expiration = DateTime.UtcNow.AddHours(2); // Short-lived tokens for tighter security

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = _configuration["Jwt:Issuer"] ?? "SafeVaultApi",
            Audience = _configuration["Jwt:Audience"] ?? "SafeVaultUsers",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            Expiration = expiration,
            Role = role
        };
    }
}