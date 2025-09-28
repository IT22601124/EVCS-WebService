namespace EvCharging.Application.DTOs;

public record CreateBookingRequest(string Nic, string StationId, DateOnly Date, TimeOnly Start, TimeOnly End);
public record UpdateBookingRequest(DateOnly Date, TimeOnly Start, TimeOnly End);
public record BookingResponse(string Id, string Nic, string StationId, DateOnly Date, TimeOnly Start, TimeOnly End, string Status, string? QrToken);
public record ScanRequest(string QrToken);
public record ScanResponse(string BookingId, string Nic, string StationId, DateOnly Date, TimeOnly Start, TimeOnly End, string Status);
