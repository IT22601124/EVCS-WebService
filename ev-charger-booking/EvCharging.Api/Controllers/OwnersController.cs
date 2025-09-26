using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;


namespace EvCharging.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _owners;


    public OwnersController(IOwnerService owners) { _owners = owners; }


    [HttpPost]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<ActionResult<OwnerResponse>> Create([FromBody] CreateOwnerRequest req)
        => Ok(await _owners.CreateAsync(req));


    [HttpGet("{nic}")]
    [Authorize]
    public async Task<ActionResult<OwnerResponse>> GetByNic(string nic)
    {
        var item = await _owners.GetByNicAsync(nic);
        return item is null ? NotFound() : Ok(item);
    }


    [HttpGet]
    [Authorize(Roles = Roles.Backoffice + "," + Roles.Operator)]
    public async Task<ActionResult<List<OwnerResponse>>> GetAll()
        => Ok(await _owners.GetAllAsync());


    [HttpPut("{nic}")]
    [Authorize(Roles = Roles.Backoffice)]
    public async Task<IActionResult> Update(string nic, [FromBody] UpdateOwnerRequest req)
    {
        await _owners.UpdateAsync(nic, req);
        return NoContent();
    }
}