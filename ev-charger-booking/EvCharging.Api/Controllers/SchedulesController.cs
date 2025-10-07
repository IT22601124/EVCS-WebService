using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Backoffice + "," + Roles.Operator + "," + Roles.Owner)]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _schedules;

    public SchedulesController(IScheduleService schedules) { _schedules = schedules; }

    [HttpPut]
    public async Task<ActionResult<ScheduleResponse>> Upsert([FromBody] UpsertScheduleRequest req)
        => Ok(await _schedules.UpsertAsync(req));

    [HttpGet]
    public async Task<ActionResult<List<ScheduleResponse>>> Get(string stationId, DateOnly date)
        => Ok(await _schedules.GetByStationAndDateAsync(stationId, date));

    [HttpGet("all")]
    public async Task<ActionResult<List<ScheduleResponse>>> GetAll()
    {
        // Get all schedules from all stations
        var allSchedules = await _schedules.GetAllAsync();
        return Ok(allSchedules);
    }

    [HttpGet("with-stations")]
    public async Task<ActionResult<List<ScheduleWithStationResponse>>> GetAllWithStations()
        => Ok(await _schedules.GetAllWithStationsAsync());

    [HttpDelete("cleanup")]
    [Authorize(Roles = Roles.Backoffice)] // Only Backoffice can cleanup
    public async Task<ActionResult<object>> CleanupInvalidSchedules()
    {
        var deletedCount = await _schedules.CleanupInvalidSchedulesAsync();
        return Ok(new { message = $"Deleted {deletedCount} invalid schedules with null StationId" });
    }
}
