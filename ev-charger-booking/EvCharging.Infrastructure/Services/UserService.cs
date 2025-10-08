using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;
using EvCharging.Infrastructure.Security;

namespace EvCharging.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _users;
        private readonly IRepository<Station> _stations;

        public UserService(IRepository<User> users, IRepository<Station> stations)
        {
            _users = users;
            _stations = stations;
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                throw new InvalidOperationException("Username and password are required");

            if (req.Role != Roles.Backoffice && req.Role != Roles.Operator && req.Role != Roles.Owner)
                throw new InvalidOperationException("Invalid role");

            var exists = (await _users.FindAsync(u => u.Username == req.Username)).Any();
            if (exists) throw new InvalidOperationException("Username already exists");

            // If an initial assignment is requested, make sure the station exists
            if (!string.IsNullOrEmpty(req.AssignedStationId))
            {
                var s = await _stations.GetByIdAsync(req.AssignedStationId!);
                if (s is null) throw new KeyNotFoundException("Station not found for assignment");
            }

            var entity = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = req.Username,
                PasswordHash = PasswordHasher.Hash(req.Password),
                Role = req.Role,
                IsActive = req.IsActive,
                AssignedStationId = req.Role == Roles.Operator ? req.AssignedStationId : null
            };

            await _users.InsertAsync(entity);
            return Map(entity);
        }

        public async Task<List<UserResponse>> GetAllAsync()
            => (await _users.GetAllAsync()).Select(Map).ToList();

        public async Task<UserResponse?> GetByUsernameAsync(string username)
        {
            var u = (await _users.FindAsync(x => x.Username == username)).FirstOrDefault();
            return u is null ? null : Map(u);
        }

        public async Task UpdateAsync(string username, UpdateUserRequest req)
        {
            var u = (await _users.FindAsync(x => x.Username == username)).FirstOrDefault()
                ?? throw new KeyNotFoundException("User not found");

            if (req.Role != Roles.Backoffice && req.Role != Roles.Operator && req.Role != Roles.Owner)
                throw new InvalidOperationException("Invalid role");

            // if changing to Operator and an AssignedStationId is provided, validate it
            if (req.Role == Roles.Operator && !string.IsNullOrWhiteSpace(req.AssignedStationId))
            {
                var st = await _stations.GetByIdAsync(req.AssignedStationId!);
                if (st is null) throw new KeyNotFoundException("Assigned station not found");
            }

            u.Role = req.Role;
            u.IsActive = req.IsActive;
            u.AssignedStationId = req.Role == Roles.Operator ? req.AssignedStationId : null;
            
            if (!string.IsNullOrWhiteSpace(req.Password))
                u.PasswordHash = PasswordHasher.Hash(req.Password);

            u.UpdatedAt = DateTime.UtcNow;
            await _users.UpdateAsync(u.Id, u);
        }

        public async Task<UserResponse> AssignToStationAsync(string username, string stationId)
        {
            var u = (await _users.FindAsync(x => x.Username == username)).FirstOrDefault()
                ?? throw new KeyNotFoundException("User not found");

            if (u.Role != Roles.Operator)
                throw new InvalidOperationException("Only Operator users can be assigned to a station");

            var st = await _stations.GetByIdAsync(stationId);
            if (st is null) throw new KeyNotFoundException("Station not found");

            u.AssignedStationId = stationId;
            u.UpdatedAt = DateTime.UtcNow;

            await _users.UpdateAsync(u.Id, u);
            return Map(u);
        }

        public async Task<UserResponse> UnassignFromStationAsync(string username)
        {
            var u = (await _users.FindAsync(x => x.Username == username)).FirstOrDefault()
                ?? throw new KeyNotFoundException("User not found");

            if (u.Role != Roles.Operator)
                throw new InvalidOperationException("Only Operator users have a station assignment");

            u.AssignedStationId = null;
            u.UpdatedAt = DateTime.UtcNow;

            await _users.UpdateAsync(u.Id, u);
            return Map(u);
        }
        public async Task<List<UserResponse>> GetOperatorsByStationAsync(string stationId)
        {
            var list = await _users.FindAsync(u =>
                u.Role == Roles.Operator && u.AssignedStationId == stationId
            );

            return list.Select(Map).ToList();
        }

        public async Task<UserResponse?> GetCurrentAsync(string usernameFromToken)
        {
            // returns the logged-in user's info (any role)
            var user = (await _users.FindAsync(u => u.Username == usernameFromToken)).FirstOrDefault();
            return user is null ? null : Map(user);
        }
        private static UserResponse Map(User u)
            => new(u.Id, u.Username, u.Role, u.IsActive, u.AssignedStationId);
    }
}
