using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;
using EvCharging.Infrastructure.Security;

namespace EvCharging.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _users;

    public UserService(IRepository<User> users) { _users = users; }

    public async Task<UserResponse> CreateAsync(CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            throw new InvalidOperationException("Username and password are required");

        if (req.Role != Roles.Backoffice && req.Role != Roles.Operator && req.Role != Roles.Owner)
            throw new InvalidOperationException("Invalid role");

        var exists = (await _users.FindAsync(u => u.Username == req.Username)).Any();
        if (exists) throw new InvalidOperationException("Username already exists");

        var entity = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Username = req.Username,
            PasswordHash = PasswordHasher.Hash(req.Password),
            Role = req.Role,
            IsActive = req.IsActive
        };

        await _users.InsertAsync(entity);
        return new UserResponse(entity.Id, entity.Username, entity.Role, entity.IsActive);
    }

    public async Task<List<UserResponse>> GetAllAsync()
        => (await _users.GetAllAsync()).Select(u => new UserResponse(u.Id, u.Username, u.Role, u.IsActive)).ToList();

    public async Task<UserResponse?> GetByUsernameAsync(string username)
    {
        var u = (await _users.FindAsync(x => x.Username == username)).FirstOrDefault();
        return u is null ? null : new UserResponse(u.Id, u.Username, u.Role, u.IsActive);
    }

    public async Task UpdateAsync(string username, UpdateUserRequest req)
    {
        var u = (await _users.FindAsync(x => x.Username == username)).FirstOrDefault()
            ?? throw new KeyNotFoundException("User not found");

        if (req.Role != Roles.Backoffice && req.Role != Roles.Operator && req.Role != Roles.Owner)
            throw new InvalidOperationException("Invalid role");

        u.Role = req.Role;
        u.IsActive = req.IsActive;
        if (!string.IsNullOrWhiteSpace(req.Password))
            u.PasswordHash = PasswordHasher.Hash(req.Password);

        u.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(u.Id, u);
    }
}
