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
    [Authorize(Roles = $"{Roles.Backoffice},{Roles.Operator}")]
    public async Task<ActionResult<StationResponse>> Create([FromBody] CreateStationRequest req)
        => Ok(await _stations.CreateAsync(req));


    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<StationResponse>>> GetAll()
        => Ok(await _stations.GetAllAsync());


    [HttpGet("with-schedules")]
    [Authorize]
    public async Task<ActionResult<List<StationWithSchedulesResponse>>> GetAllWithSchedules([FromQuery] DateOnly? date = null)
        => Ok(await _stations.GetAllWithSchedulesAsync(date));


    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<StationResponse>> GetById(string id)
    {
        var item = await _stations.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = $"{Roles.Backoffice},{Roles.Operator}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateStationRequest req)
    {
        await _stations.UpdateAsync(id, req);
        return NoContent();
    }
}