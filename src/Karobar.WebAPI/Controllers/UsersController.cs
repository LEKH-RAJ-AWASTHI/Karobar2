using Karobar.Application.Interfaces;
using Karobar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karobar.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string search = null)
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            usersQuery = usersQuery.Where(u => u.Email.Contains(search) || u.FullName.Contains(search));
        }

        var users = await usersQuery
            .Select(u => new { u.Id, u.Email, u.FullName })
            .ToListAsync();

        var userDtos = new List<object>();
        foreach (var u in users)
        {
            var userEntity = await _userManager.FindByIdAsync(u.Id);
            var roles = await _userManager.GetRolesAsync(userEntity);
            var role = roles.FirstOrDefault() ?? "User";
            userDtos.Add(new { u.Id, u.Email, u.FullName, Role = role });
        }
        
        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        return Ok(new { user.Id, user.Email, user.FullName, Role = role });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, dto.Role);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);
        return NoContent();
    }
}

public class UpdateUserDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
