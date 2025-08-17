using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTO;
using Serilog;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin-dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : BaseDashboardController
    {
        public AdminDashboardController(AppDbContext context) : base(context) { }

        // ✅ Fetch all active non-admin users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Role != "Admin" && u.IsActive)
                    .Select(u => new { u.Id, u.FullName, u.Email })
                    .ToListAsync();

                Log.Information("Admin fetched all active non-admin users. Count: {Count}", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching users");
                return StatusCode(500, new { message = "Error fetching users", error = ex.Message });
            }
        }

        // ✅ Admin dashboard summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetAdminSummary()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var recentUsers = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7));

                var completed = await _context.UserTasks.CountAsync(t => t.Status == "Completed");
                var pending = await _context.UserTasks.CountAsync(t => t.Status == "Pending");
                var inProgress = await _context.UserTasks.CountAsync(t => t.Status == "In Progress");

                var recentTasks = await _context.UserTasks
                    .Include(t => t.User)
                    .OrderByDescending(t => t.Id)
                    .Take(10)
                    .Select(t => new { t.Id, t.Title, t.Status, User = t.User != null ? t.User.FullName : "Unknown" })
                    .ToListAsync();

                var users = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new { u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAt })
                    .ToListAsync();

                Log.Information("Admin dashboard summary fetched successfully.");

                return Ok(new
                {
                    taskStats = new { completed, pending, inProgress },
                    userStats = new { total = totalUsers, active = activeUsers, @new = recentUsers },
                    recentTasks,
                    users
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving admin summary");
                return StatusCode(500, new { message = "Error retrieving admin summary", error = ex.Message });
            }
        }

        // ✅ Toggle user activation
        [HttpPut("toggle-active/{id}")]
        public async Task<IActionResult> ToggleUserActivation(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });
                if (user.Role == "Admin") return BadRequest(new { message = "Cannot change activation status for Admin users" });

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                Log.Information("User ID {Id} activation toggled. New status: {Status}", user.Id, user.IsActive);
                return Ok(new { message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully", userId = user.Id, isActive = user.IsActive });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error toggling user activation for ID: {Id}", id);
                return StatusCode(500, new { message = "Error toggling user activation", error = ex.Message });
            }
        }

        // ✅ Delete task
        [HttpDelete("delete-task/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.UserTasks.FindAsync(id);
                if (task == null) return NotFound(new { message = "Task not found" });

                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();

                Log.Information("Admin deleted task ID: {Id}", id);
                return Ok(new { message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting task ID: {Id}", id);
                return StatusCode(500, new { message = "Error deleting task", error = ex.Message });
            }
        }

        // ✅ Get task details by ID
        [HttpGet("tasks/{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var task = await _context.UserTasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
                if (task == null) return NotFound(new { message = "Task not found" });

                Log.Information("Admin fetched details for task ID: {Id}", id);
                return Ok(new { task.Id, task.Title, task.Description, task.DueDate, task.Status, task.Priority });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving task ID: {Id}", id);
                return StatusCode(500, new { message = "Error retrieving task", error = ex.Message });
            }
        }

        // ✅ Create task by Admin
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto model)
        {
            try
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null || user.Role == "Admin" || !user.IsActive)
                    return BadRequest(new { message = "Invalid or inactive user" });

                var task = new UserTask
                {
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    Status = model.Status,
                    Priority = model.Priority,
                    UserId = model.UserId
                };

                _context.UserTasks.Add(task);
                await _context.SaveChangesAsync();

                Log.Information("Admin created task '{Title}' for user ID: {UserId}", model.Title, model.UserId);
                return Ok(new { message = "Task created successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating task for user ID: {UserId}", model.UserId);
                return StatusCode(500, new { message = "Error creating task", error = ex.Message });
            }
        }
    }
}