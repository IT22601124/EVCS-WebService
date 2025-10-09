using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;
using System.ComponentModel.DataAnnotations;

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
    public async Task<ActionResult<List<ScheduleResponse>>> Get(
        [FromQuery] string? stationId = null, 
        [FromQuery] DateOnly? date = null)
    {
        // If no parameters provided, return all schedules
        if (string.IsNullOrWhiteSpace(stationId) && date == null)
        {
            return Ok(await _schedules.GetAllAsync());
        }
        
        // If stationId provided but no date, require date
        if (!string.IsNullOrWhiteSpace(stationId) && date == null)
        {
            return BadRequest("Date is required when stationId is provided");
        }
        
        // If date provided but no stationId, require stationId
        if (string.IsNullOrWhiteSpace(stationId) && date != null)
        {
            return BadRequest("StationId is required when date is provided");
        }
        
        // Both parameters provided - we know they're not null at this point
        return Ok(await _schedules.GetByStationAndDateAsync(stationId!, date!.Value));
    }

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
