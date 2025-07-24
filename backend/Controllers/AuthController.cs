using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) => _context = context;

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.FullName) ||
                string.IsNullOrWhiteSpace(newUser.Email) ||
                string.IsNullOrWhiteSpace(newUser.PasswordHash))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == newUser.Email))
                return BadRequest(new { message = "Email already exists" });

            newUser.PasswordHash = ComputeSha256Hash(newUser.PasswordHash);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Signup successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginUser)
        {
            if (string.IsNullOrWhiteSpace(loginUser.Email) ||
                string.IsNullOrWhiteSpace(loginUser.PasswordHash))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            string hashed = ComputeSha256Hash(loginUser.PasswordHash);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginUser.Email && u.PasswordHash == hashed);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Login successful" });
        }

        private static string ComputeSha256Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
