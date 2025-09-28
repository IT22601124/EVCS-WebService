using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;

namespace EvCharging.Application.Contracts;

public interface IAuthService
{
    Task<(string token, DateTime expiresAt, string role)> LoginAsync(string username, string password);
}

public interface IOwnerService
{
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest req);
    Task<OwnerResponse?> GetByNicAsync(string nic);
    Task<List<OwnerResponse>> GetAllAsync();
    Task UpdateAsync(string nic, UpdateOwnerRequest req);
}

public interface IStationService
{
    Task<StationResponse> CreateAsync(CreateStationRequest req);
    Task<List<StationResponse>> GetAllAsync();
    Task<StationResponse?> GetByIdAsync(string id);
    Task UpdateAsync(string id, UpdateStationRequest req);
}

public interface IScheduleService
{
    Task<ScheduleResponse> UpsertAsync(UpsertScheduleRequest req);
    Task<List<ScheduleResponse>> GetByStationAndDateAsync(string stationId, DateOnly date);
}

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest req);
    Task<BookingResponse?> GetByIdAsync(string id);
    Task<List<BookingResponse>> GetByOwnerAsync(string nic);
    Task<List<BookingResponse>> GetByStationAndDateAsync(string stationId, DateOnly date);
    Task UpdateAsync(string id, UpdateBookingRequest req);
    Task CancelAsync(string id);
    Task<BookingResponse> ApproveAsync(string id);
    Task<ScanResponse> ScanAsync(string qrToken);
    Task FinalizeAsync(string id);
}