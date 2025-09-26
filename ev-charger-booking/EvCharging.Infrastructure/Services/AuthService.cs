using EvCharging.Application.Contracts;
using EvCharging.Domain.Entities;
using EvCharging.Infrastructure.Security;
using EvCharging.Application.DTOs;


namespace EvCharging.Infrastructure.Services;


public class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly JwtTokenService _jwt;


    public AuthService(IRepository<User> users, JwtTokenService jwt)
    {
        _users = users; _jwt = jwt;
    }


    public async Task<(string token, DateTime expiresAt, string role)> LoginAsync(string username, string password)
    {
        var user = (await _users.FindAsync(u => u.Username == username)).FirstOrDefault();
        if (user is null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
        throw new UnauthorizedAccessException("Invalid credentials");


        var (token, exp) = _jwt.CreateToken(user.Username, user.Role);
        return (token, exp, user.Role);
    }
}