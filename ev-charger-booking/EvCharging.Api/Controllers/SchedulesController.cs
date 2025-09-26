using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _schedules;

    public SchedulesController(IScheduleService schedules) { _schedules = schedules; }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ScheduleResponse>> Upsert([FromBody] UpsertScheduleRequest req)
        => Ok(await _schedules.UpsertAsync(req));

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<ScheduleResponse>>> Get(string stationId, DateOnly date)
        => Ok(await _schedules.GetByStationAndDateAsync(stationId, date));
}