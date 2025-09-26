using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace EvCharging.Infrastructure.Security;


public class JwtTokenService
{
    private readonly IConfiguration _config;


    public JwtTokenService(IConfiguration config) { _config = config; }


    public (string token, DateTime expiresAt) CreateToken(string username, string role)
    {
        var issuer = _config["Jwt:Issuer"]!;
        var audience = _config["Jwt:Audience"]!;
        var secret = _config["Jwt:Secret"]!;
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);


        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };


        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);
        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenStr, expires);
    }
}