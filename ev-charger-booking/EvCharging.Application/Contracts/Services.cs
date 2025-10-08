using EvCharging.Application.DTOs;

namespace EvCharging.Application.Contracts;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string username, string password);
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
    Task<List<StationResponse>> GetByAssignedOperatorAsync(string operatorUsername);
    Task DeleteAsync(string id);
    Task<List<StationWithSchedulesResponse>> GetAllWithSchedulesAsync(DateOnly? date = null);
    Task<List<StationWithSchedulesResponse>> GetAllWithWeeklySchedulesAsync(DateOnly? startDate = null);
}

public interface IScheduleService
{
    Task<ScheduleResponse> UpsertAsync(UpsertScheduleRequest req);
    Task<List<ScheduleResponse>> GetByStationAndDateAsync(string stationId, DateOnly date);
    Task<List<ScheduleResponse>> GetAllAsync();
    Task<List<ScheduleWithStationResponse>> GetAllWithStationsAsync();
    Task<int> CleanupInvalidSchedulesAsync();
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

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest req);
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByUsernameAsync(string username);
    Task UpdateAsync(string username, UpdateUserRequest req);

    Task<UserResponse> AssignToStationAsync(string username, string stationId);
    Task<UserResponse> UnassignFromStationAsync(string username);
    Task<List<UserResponse>> GetOperatorsByStationAsync(string stationId);
    Task<UserResponse?> GetCurrentAsync(string usernameFromToken);
}
