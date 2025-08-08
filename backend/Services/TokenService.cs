using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Backend.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public string GenerateToken(User user)
        {
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                };

                var keyString = _config["Jwt:Key"];
                if (string.IsNullOrEmpty(keyString))
                {
                    Log.Error("JWT Key is missing in configuration.");
                    throw new Exception("JWT Key not configured.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Issuer"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                Log.Information("JWT token generated for user {Email}", user.Email);
                return jwt;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate JWT token for user {Email}", user.Email);
                throw; // re-throw so calling method can handle it
            }
        }
    }
}
