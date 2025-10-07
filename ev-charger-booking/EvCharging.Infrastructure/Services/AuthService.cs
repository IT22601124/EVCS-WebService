using EvCharging.Application.Contracts;
using EvCharging.Domain.Entities;
using EvCharging.Infrastructure.Security;
using EvCharging.Application.DTOs;


namespace EvCharging.Infrastructure.Services;


public class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Station> _stations;
    private readonly JwtTokenService _jwt;


    public AuthService(IRepository<User> users, IRepository<Station> stations, JwtTokenService jwt)
    {
        _users = users;
        _stations = stations;
        _jwt = jwt;
    }

    public async Task<(string token, DateTime expiresAt, string role)> LoginAsync(string username, string password)
    {
        var user = (await _users.FindAsync(u => u.Username == username)).FirstOrDefault();
        if (user is null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
        throw new UnauthorizedAccessException("Invalid credentials");

        // find station(s) assigned to this operator
        var assignedStationIds = await _stations.FindAsync(s => s.AssignedOperators.Contains(username));
        var stationIds = assignedStationIds.Select(s => s.Id).ToList();

        var (token, exp) = _jwt.CreateToken(user.Username, user.Role, stationIds);
        return (token, exp, user.Role);
    }
}