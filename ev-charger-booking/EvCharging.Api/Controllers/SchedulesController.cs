using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Backoffice + "," + Roles.Operator)] // tightened access
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
}
