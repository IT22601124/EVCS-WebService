using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Backoffice)]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) { _users = users; }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest req)
        => Ok(await _users.CreateAsync(req));

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
        => Ok(await _users.GetAllAsync());

    [HttpGet("{username}")]
    public async Task<ActionResult<UserResponse>> GetByUsername(string username)
    {
        var u = await _users.GetByUsernameAsync(username);
        return u is null ? NotFound() : Ok(u);
    }

    [HttpPut("{username}")]
    public async Task<IActionResult> Update(string username, [FromBody] UpdateUserRequest req)
    { await _users.UpdateAsync(username, req); return NoContent(); }
}
