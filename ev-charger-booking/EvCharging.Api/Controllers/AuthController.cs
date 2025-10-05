using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;


namespace EvCharging.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;


    public AuthController(IAuthService auth) { _auth = auth; }


    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var (token, exp, role) = await _auth.LoginAsync(req.Username, req.Password);
        return Ok(new LoginResponse(token, exp, role, req.Username));
    }
}