using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;

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
    public async Task<ActionResult<List<StationResponse>>> GetAll()
    {
        var role = User.FindFirst("role")?.Value;
        var username = User.Identity?.Name;

        if (role == Roles.Operator && !string.IsNullOrEmpty(username))
        {
            var items = await _stations.GetByAssignedOperatorAsync(username);
            return Ok(items);
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
}