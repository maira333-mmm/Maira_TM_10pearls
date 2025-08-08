using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Data;
using Backend.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ✅ Signup Endpoint (Safe version: force User role)
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User newUser)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == newUser.Email))
                {
                    Log.Warning("Signup attempt with existing email: {Email}", newUser.Email);
                    return BadRequest(new { message = "Email already exists" });
                }

                newUser.PasswordHash = ComputeSha256Hash(newUser.PasswordHash);
                newUser.Role = "User"; // ✅ Prevent role override from frontend
                newUser.IsActive = true;
                newUser.CreatedAt = DateTime.UtcNow;

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                Log.Information("New user signed up successfully: {Email}", newUser.Email);
                return Ok(new { message = "Signup successful" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during signup");
                return StatusCode(500, new { message = "An error occurred during signup" });
            }
        }

        // ✅ Login Endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginUser)
        {
            try
            {
                string hashedPassword = ComputeSha256Hash(loginUser.PasswordHash);

                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == loginUser.Email.ToLower().Trim() &&
                    u.PasswordHash == hashedPassword);

                if (user == null)
                {
                    Log.Warning("Login failed for email: {Email}", loginUser.Email);
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // ✅ Check if account is deactivated
                if (!user.IsActive)
                {
                    Log.Warning("Deactivated account tried to login: {Email}", user.Email);
                    return Unauthorized(new { message = "Your account has been deactivated. Please contact admin." });
                }

                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                string token = GenerateJwtToken(user);

                Log.Information("User logged in: {Email}", user.Email);
                return Ok(new
                {
                    token,
                    role = user.Role,
                    fullName = user.FullName,
                    email = user.Email,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during login for email: {Email}", loginUser.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        // 🔐 Password Hashing Method
        private static string ComputeSha256Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // 🔐 Token Generator
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
