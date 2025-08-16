using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Serilog;
using System.Security.Claims;

namespace Backend.Controllers
{
    public abstract class BaseDashboardController : ControllerBase
    {
        protected readonly AppDbContext _context;

        protected BaseDashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches dashboard summary for a given userId.
        /// Returns null if user not found.
        /// </summary>
        protected async Task<object?> GetUserSummaryAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var userTasks = _context.UserTasks.Where(t => t.UserId == user.Id);

            return new
            {
                completed = await userTasks.CountAsync(t => t.Status == "Completed"),
                inProgress = await userTasks.CountAsync(t => t.Status == "In Progress"),
                pending = await userTasks.CountAsync(t => t.Status == "Pending"),
                user = new { fullName = user.FullName, email = user.Email }
            };
        }

        /// <summary>
        /// Extracts current user from JWT claims and returns nullable dashboard summary.
        /// </summary>
        protected async Task<object?> GetCurrentUserSummaryAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(email))
            {
                Log.Warning("Token received without email claim.");
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Log.Warning("User not found for email: {Email}", email);
                return null;
            }

            Log.Information("Fetching dashboard summary for user {UserId}", user.Id);
            return await GetUserSummaryAsync(user.Id);
        }
    }
}
