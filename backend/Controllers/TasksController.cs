using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Data;
using Backend.Models;
using Backend.DTO;
using Microsoft.EntityFrameworkCore;
using Serilog;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Create Task
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

            Log.Information("Task created successfully for UserId: {UserId}", userId);
            return Ok(new { message = "Task created successfully!" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CreateTask Error");
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }

    // ✅ Get All Tasks
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

            Log.Information("Fetched task list for UserId: {UserId}", userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetTasks Error");
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }

    // ✅ Get Task by ID
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetTaskById(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid user token" });

            int userId = int.Parse(userIdClaim.Value);

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                Log.Warning("Task not found or unauthorized. TaskId: {TaskId}, UserId: {UserId}", id, userId);
                return NotFound(new { message = "Task not found or unauthorized" });
            }

            Log.Information("Fetched task {TaskId} for UserId: {UserId}", id, userId);
            return Ok(task);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetTaskById Error");
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }

    // ✅ Update Task
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] CreateTaskDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid user token" });

            int userId = int.Parse(userIdClaim.Value);

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                Log.Warning("Update attempt on nonexistent task {TaskId} by UserId {UserId}", id, userId);
                return NotFound(new { message = "Task not found or unauthorized" });
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;
            task.Status = dto.Status;
            task.Priority = dto.Priority;

            await _context.SaveChangesAsync();

            Log.Information("Task {TaskId} updated by UserId {UserId}", id, userId);
            return Ok(new { message = "Task updated successfully!" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UpdateTask Error");
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }

    // ✅ Delete Task
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

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                Log.Warning("Delete attempt on nonexistent task {TaskId} by UserId {UserId}", id, userId);
                return NotFound(new { message = "Task not found or unauthorized" });
            }

            _context.UserTasks.Remove(task);
            await _context.SaveChangesAsync();

            Log.Information("Task {TaskId} deleted by UserId {UserId}", id, userId);
            return Ok(new { message = "Task deleted successfully" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DeleteTask Error");
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }
}
