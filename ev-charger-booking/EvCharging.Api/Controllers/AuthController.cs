using Microsoft.AspNetCore.Mvc;
using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;


namespace EvCharging.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;


    public AuthController(IAuthService auth, ILogger<AuthController> logger) 
    { 
        _auth = auth; 
        _logger = logger;
    }


    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        try
        {
            var result = await _auth.LoginAsync(req.Username, req.Password);
            _logger.LogInformation("User {Username} logged in successfully with role {Role}, IsOwner: {IsOwner}", 
                result.Username, result.Role, result.IsOwner);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login attempt failed for user: {Username}. Error: {Error}", req.Username, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for user: {Username}", req.Username);
            return StatusCode(500, new { message = "Login failed" });
        }
    }
}