using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetSummary()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);

            if (email == null)
            {
                _logger.LogWarning("Token received without email claim.");
                return Unauthorized("Invalid token");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return Unauthorized("User not found");
            }

            _logger.LogInformation("Fetching dashboard summary for user {UserId}", user.Id);

            var tasks = _context.UserTasks.Where(t => t.UserId == user.Id);

            var result = new
            {
                completed = await tasks.CountAsync(t => t.Status == "Completed"),
                inProgress = await tasks.CountAsync(t => t.Status == "In Progress"),
                pending = await tasks.CountAsync(t => t.Status == "Pending"),
                user = new
                {
                    fullName = user.FullName,
                    email = user.Email
                }
            };

            _logger.LogInformation("Summary fetched for user {UserId}", user.Id);

            return Ok(result);
        }
    }
}
