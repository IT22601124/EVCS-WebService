using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;
using EvCharging.Infrastructure.Security;
using EvCharging.Infrastructure.Persistence;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IOwnerService _owners;
    private readonly IRepository<User> _users;
    private readonly MongoDbContext _dbContext;

    public RegistrationController(IOwnerService owners, IRepository<User> users, MongoDbContext dbContext)
    { 
        _owners = owners; 
        _users = users;
        _dbContext = dbContext;
    }

    // Public owner sign-up (no auth): creates EvOwner + User (Owner role)
    [HttpPost("owner")]
    [ProducesResponseType(typeof(OwnerRegistrationResponse), 200)]
    public async Task<ActionResult<OwnerRegistrationResponse>> RegisterOwner([FromBody] CreateOwnerRequest req)
    {
        var owner = await _owners.CreateAsync(req);

        var userRole = Roles.Owner;
        var exists = (await _users.FindAsync(u => u.Username == req.Nic)).Any();
        if (!exists)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = req.Nic,
                PasswordHash = PasswordHasher.Hash(req.Password), // Use provided password instead of NIC
                Role = userRole,
                IsActive = true,
                Nic = req.Nic,
                FullName = req.FullName,
                Email = req.Email,
                Phone = req.Phone
            };
            await _users.InsertAsync(user);
        }

        // Return response with role information
        var response = new OwnerRegistrationResponse(
            owner.Nic, 
            owner.FullName, 
            owner.Email, 
            owner.Phone, 
            owner.IsActive, 
            userRole
        );
        Console.WriteLine($"Registered new owner: {owner.Nic} with role {userRole}");

        return Ok(response);
    }

    // Admin registration endpoint for creating Backoffice and Operator users
    [HttpPost("admin")]
    [ProducesResponseType(typeof(UserRegistrationResponse), 200)]
    public async Task<ActionResult<UserRegistrationResponse>> RegisterAdmin([FromBody] CreateAdminUserRequest req)
    {
        try
        {
            // Validate role
            if (req.Role != Roles.Backoffice && req.Role != Roles.Operator)
            {
                return BadRequest(new { message = "Invalid role. Only Backoffice and Operator roles are allowed." });
            }

            // Check if user already exists
            var existingUser = (await _users.FindAsync(u => u.Username == req.Username)).FirstOrDefault();
            if (existingUser != null)
            {
                return Conflict(new { message = "User already exists with this username." });
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = req.Username,
                PasswordHash = PasswordHasher.Hash(req.Password),
                Role = req.Role,
                IsActive = true,
                Nic = req.Nic,
                FullName = req.FullName,
                Email = req.Email,
                Phone = req.Phone,
                AssignedStations = req.AssignedStations ?? new List<string>()
            };

            await _users.InsertAsync(user);

            var response = new UserRegistrationResponse(
                user.Username,
                user.Role,
                user.Nic,
                user.FullName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.AssignedStations
            );

            Console.WriteLine($"Registered new {req.Role} user: {user.Username}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration failed: {ex.Message}");
            return BadRequest(new { message = "Registration failed", error = ex.Message });
        }
    }

    // Assign station to operator
    [HttpPost("assign-station")]
    [Authorize(Roles = Roles.Backoffice)]
    [ProducesResponseType(typeof(AssignStationResponse), 200)]
    public async Task<ActionResult<AssignStationResponse>> AssignStation([FromBody] AssignStationRequest req)
    {
        try
        {
            // Find the operator
            var user = (await _users.FindAsync(u => u.Username == req.OperatorUsername && u.Role == Roles.Operator)).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "Operator not found." });
            }

            // Check if station is already assigned
            if (user.AssignedStations.Contains(req.StationId))
            {
                return BadRequest(new { message = "Station is already assigned to this operator." });
            }

            // Add station to operator's assigned stations
            user.AssignedStations.Add(req.StationId);
            await _users.UpdateAsync(user.Id, user);

            var response = new AssignStationResponse(user.Username, user.AssignedStations);
            Console.WriteLine($"Assigned station {req.StationId} to operator {user.Username}");
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Station assignment failed: {ex.Message}");
            return BadRequest(new { message = "Station assignment failed", error = ex.Message });
        }
    }

    // Temporary endpoint to clean database (remove in production)
    [HttpPost("cleanup-database")]
    public async Task<ActionResult> CleanupDatabase()
    {
        try
        {
            var db = _dbContext.Database;
            
            // Drop all collections to start fresh
            await db.DropCollectionAsync("User");
            await db.DropCollectionAsync("EvOwner");
            await db.DropCollectionAsync("Station");
            await db.DropCollectionAsync("StationSchedule");
            await db.DropCollectionAsync("Booking");
            
            return Ok(new { message = "Database cleaned successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Cleanup failed", error = ex.Message });
        }
    }
}
