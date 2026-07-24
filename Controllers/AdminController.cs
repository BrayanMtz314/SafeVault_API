using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SafeVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// SECURITY: This attribute enforces that ONLY valid JWTs containing the "Admin" role claim can enter!
[Authorize(Roles = "Admin")] 
public class AdminController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllRegisteredUsers()
    {
        // Admins can audit all user accounts currently registered in SafeVault
        var users = await _userManager.Users
            .Select(u => new { u.Id, u.Email, u.EmailConfirmed })
            .ToListAsync();

        return Ok(users);
    }
}