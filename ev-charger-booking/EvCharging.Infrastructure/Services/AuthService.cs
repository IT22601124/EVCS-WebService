using EvCharging.Application.Contracts;
using EvCharging.Domain.Entities;
using EvCharging.Infrastructure.Security;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;


namespace EvCharging.Infrastructure.Services;


public class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Station> _stations;
    private readonly IRepository<EvOwner> _owners;
    private readonly JwtTokenService _jwt;


    public AuthService(IRepository<User> users, IRepository<Station> stations, IRepository<EvOwner> owners, JwtTokenService jwt)
    {
        _users = users;
        _stations = stations;
        
        _owners = owners;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var user = (await _users.FindAsync(u => u.Username == username)).FirstOrDefault();
        if (user is null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        // find station(s) assigned to this operator
        var assignedStationIds = await _stations.FindAsync(s => s.AssignedOperators.Contains(username));
        var stationIds = assignedStationIds.Select(s => s.Id).ToList();
        var (token, exp) = _jwt.CreateToken(user.Username, user.Role, stationIds);
        
        // Check if user is an Owner and get owner details
        bool isOwner = false;
        string? ownerNic = null;
        string nic = user.Nic;
        string fullName = user.FullName;
        string email = user.Email;
        string phone = user.Phone;
        
        if (user.Role == Roles.Owner)
        {
            // For Owner role, try to find the corresponding EvOwner record
            var ownerRecord = (await _owners.FindAsync(o => o.Nic == username || o.Email == username)).FirstOrDefault();
            if (ownerRecord != null && ownerRecord.IsActive)
            {
                isOwner = true;
                ownerNic = ownerRecord.Nic;
                // Use owner record details if available, otherwise fall back to user details
                nic = ownerRecord.Nic;
                fullName = ownerRecord.FullName;
                email = ownerRecord.Email;
                phone = ownerRecord.Phone;
            }
        }

        return new LoginResponse(
            nic,
            fullName,
            email,
            phone,
            user.IsActive,
            user.Role,
            token,
            exp,
            user.Username,
            isOwner,
            ownerNic
        );
    }
}