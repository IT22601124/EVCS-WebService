using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;
using System.Security.Claims;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) { _users = users; }

    // Admin only
    [HttpPost]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest req)
        => Ok(await _users.CreateAsync(req));

    // Admin only
    [HttpGet]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
        => Ok(await _users.GetAllAsync());

    // Admin only: keep this restricted to Backoffice
    [HttpGet("{username}")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<UserResponse>> GetByUsername(string username)
    {
        var u = await _users.GetByUsernameAsync(username);
        return u is null ? NotFound() : Ok(u);
    }

    // Admin only
    [HttpPut("{username}")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<IActionResult> Update(string username, [FromBody] UpdateUserRequest req)
    {
        await _users.UpdateAsync(username, req);
        return NoContent();
    }

    // body-based assign
    [HttpPost("{username}/assign")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<UserResponse>> AssignToStation(string username, [FromBody] AssignStationRequest req)
        => Ok(await _users.AssignToStationAsync(username, req.StationId));

    // route-based assign so frontend can call /assign/{stationId}
    [HttpPost("{username}/assign/{stationId}")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<UserResponse>> AssignToStationViaRoute(string username, string stationId)
        => Ok(await _users.AssignToStationAsync(username, stationId));

    // unassign
    [HttpPost("{username}/unassign")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<UserResponse>> UnassignFromStation(string username)
        => Ok(await _users.UnassignFromStationAsync(username));

    // Admin only
    [HttpGet("operators/by-station/{stationId}")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<List<UserResponse>>> GetOperatorsByStation(string stationId)
        => Ok(await _users.GetOperatorsByStationAsync(stationId));

    // Lets any authenticated user fetch their own assignment/profile
    [HttpGet("me")]
    [Authorize] // authenticated users (no Backoffice role required)
    public async Task<ActionResult<UserResponse>> Me()
    {
        // Try common claim names. Adjust if your tokens use different names.
        var username = User.FindFirstValue(ClaimTypes.Name)
                    ?? User.FindFirstValue("username")
                    ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized();

        var me = await _users.GetCurrentAsync(username);
        return me is null ? NotFound() : Ok(me);
    }

}

// AssignStationRequest is defined in EvCharging.Application.DTOs.UserDtos

