using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Data;
using Backend.Models;
using Backend.DTO;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid user token" });

            int userId = int.Parse(userIdClaim.Value);

            var task = new UserTask
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = dto.Status,
                Priority = dto.Priority,
                UserId = userId
            };

            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task created successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("CreateTask Error:\n" + ex.ToString());
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetTasks()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid user token" });

            int userId = int.Parse(userIdClaim.Value);

            var tasks = await _context.UserTasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.DueDate)
                .ToListAsync();

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetTasks Error:\n" + ex.ToString());
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }

    // ✅ DELETE TASK
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid user token" });

            int userId = int.Parse(userIdClaim.Value);

            var task = await _context.UserTasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound(new { message = "Task not found or unauthorized" });

            _context.UserTasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task deleted successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("DeleteTask Error:\n" + ex.ToString());
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }
}
