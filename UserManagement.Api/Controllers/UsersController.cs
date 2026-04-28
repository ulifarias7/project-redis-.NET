using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Dtos;
using UserManagement.Api.Services.Interfaces;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await userService.UpdateUserAsync(id, dto);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await userService.DeleteUserAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
