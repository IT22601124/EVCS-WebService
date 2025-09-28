using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;
    public BookingsController(IBookingService bookings) { _bookings = bookings; }

    [HttpPost]
    [Authorize] // owner creates, but also allow backoffice
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest req)
        => Ok(await _bookings.CreateAsync(req));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<BookingResponse>> GetById(string id)
    {
        var item = await _bookings.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-owner/{nic}")]
    [Authorize]
    public async Task<ActionResult<List<BookingResponse>>> GetByOwner(string nic)
        => Ok(await _bookings.GetByOwnerAsync(nic));

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<BookingResponse>>> GetByStationAndDate([FromQuery] string stationId, [FromQuery] DateOnly date)
        => Ok(await _bookings.GetByStationAndDateAsync(stationId, date));

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBookingRequest req)
    { await _bookings.UpdateAsync(id, req); return NoContent(); }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Cancel(string id)
    { await _bookings.CancelAsync(id); return NoContent(); }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<BookingResponse>> Approve(string id)
        => Ok(await _bookings.ApproveAsync(id));

    [HttpPost("scan")]
    [Authorize(Roles = Roles.Operator + "," + Roles.Backoffice)]
    public async Task<ActionResult<ScanResponse>> Scan([FromBody] ScanRequest req)
        => Ok(await _bookings.ScanAsync(req.QrToken));

    [HttpPost("{id}/finalize")]
    [Authorize(Roles = Roles.Operator + "," + Roles.Backoffice)]
    public async Task<IActionResult> FinalizeBooking(string id)
    { await _bookings.FinalizeAsync(id); return NoContent(); }
}
