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
    [ProducesResponseType(typeof(OwnerResponse), 200)]
    public async Task<ActionResult<OwnerResponse>> RegisterOwner([FromBody] CreateOwnerRequest req)
    {
        var owner = await _owners.CreateAsync(req);

        var exists = (await _users.FindAsync(u => u.Username == req.Nic)).Any();
        if (!exists)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = req.Nic,
                PasswordHash = PasswordHasher.Hash(req.Nic), // simple default; front-end can prompt change
                Role = Roles.Owner,
                IsActive = true
            };
            await _users.InsertAsync(user);
        }

        return Ok(owner);
    }
}
