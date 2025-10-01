using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;
using EvCharging.Infrastructure.Security;

namespace EvCharging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IOwnerService _owners;
    private readonly IRepository<User> _users;

    public RegistrationController(IOwnerService owners, IRepository<User> users)
    { _owners = owners; _users = users; }

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
                IsActive = true
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
}
