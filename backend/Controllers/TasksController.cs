using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.DTO;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : BaseTaskController
    {
        public TasksController(AppDbContext context) : base(context) { }

        // ✅ Create Task
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid user token" });

                var task = new UserTask
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    DueDate = dto.DueDate,
                    Status = dto.Status,
                    Priority = dto.Priority,
                    UserId = userId.Value
                };

                _context.UserTasks.Add(task);
                await _context.SaveChangesAsync();

                Log.Information("Task created successfully for UserId: {UserId}", userId);
                return Ok(new { message = "Task created successfully!" });
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(CreateTask));
            }
        }

        // ✅ Get All Tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid user token" });

                var tasks = await _context.UserTasks
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.DueDate)
                    .ToListAsync();

                Log.Information("Fetched task list for UserId: {UserId}", userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(GetTasks));
            }
        }

        // ✅ Get Task by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid user token" });

                var task = await _context.UserTasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    Log.Warning("Task not found or unauthorized. TaskId: {TaskId}, UserId: {UserId}", id, userId);
                    return NotFound(new { message = "Task not found or unauthorized" });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(GetTaskById));
            }
        }

        // ✅ Update Task
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] CreateTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid user token" });

                var task = await _context.UserTasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
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
                return HandleServerError(ex, nameof(UpdateTask));
            }
        }

        // ✅ Delete Task
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "Invalid user token" });

                var task = await _context.UserTasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found or unauthorized" });
                }

                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();

                Log.Information("Task {TaskId} deleted by UserId {UserId}", id, userId);
                return Ok(new { message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(DeleteTask));
            }
        }
    }
}