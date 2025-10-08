namespace EvCharging.Application.DTOs;

public record CreateBookingRequest(string Nic, string StationId, DateOnly Date, TimeOnly Start, TimeOnly End);
public record UpdateBookingRequest(DateOnly Date, TimeOnly Start, TimeOnly End);

// Enhanced BookingResponse with owner and station details
public record BookingResponse(
    string Id,
    string Nic,
    string OwnerName,
    string OwnerEmail,
    string OwnerPhone,
    string StationId,
    string StationName,
    string StationAddress,
    string StationType,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string Status,
    string? QrToken,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ScanRequest(string QrToken);

// Enhanced ScanResponse with owner and station details
public record ScanResponse(
    string BookingId,
    string Nic,
    string OwnerName,
    string StationId,
    string StationName,
    string StationAddress,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string Status
);
