using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;
using System.Security.Claims;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IStationService _stations;
    public StationsController(IStationService stations) { _stations = stations; }


    [HttpPost]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<StationResponse>> Create([FromBody] CreateStationRequest req)
        => Ok(await _stations.CreateAsync(req));


    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<StationResponse>>> GetAll([FromServices] IUserService users)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;

        // If Operator, return ONLY their assigned station (if any)
        if (role == Roles.Operator && !string.IsNullOrWhiteSpace(username))
        {
            var me = await users.GetCurrentAsync(username); // includes AssignedStationId
            if (me?.AssignedStationId is string sid && !string.IsNullOrWhiteSpace(sid))
            {
                var st = await _stations.GetByIdAsync(sid);
                // if not found, return empty list to front-end
                return Ok(st is null ? new List<StationResponse>() : new List<StationResponse> { st });
            }
            return Ok(new List<StationResponse>()); // no assignment
        }

        // Backoffice sees all
        return Ok(await _stations.GetAllAsync());
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<StationResponse>> GetById(string id)
    {
        var item = await _stations.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateStationRequest req)
    {
        await _stations.UpdateAsync(id, req);
        return NoContent();
    }

    [HttpGet("{id}/operators")]
    [Authorize(Roles = Roles.Backoffice + "," + Roles.Operator)]
    public async Task<ActionResult<List<UserResponse>>> GetOperatorsForStation(
        [FromServices] IUserService users,
        string id)
        => Ok(await users.GetOperatorsByStationAsync(id));

}