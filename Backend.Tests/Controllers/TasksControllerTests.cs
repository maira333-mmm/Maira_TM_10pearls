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
        private const string InvalidUserTokenMessage = "Invalid user token";
        private const string TaskNotFoundMessage = "Task not found or unauthorized";
        private const string TaskCreatedMessage = "Task created successfully!";
        private const string TaskUpdatedMessage = "Task updated successfully!";
        private const string TaskDeletedMessage = "Task deleted successfully";

        public TasksController(AppDbContext context) : base(context) { }

        // Central error handler
        private ObjectResult HandleServerError(Exception ex, string method)
        {
            Log.Error(ex, "Error in {Method}", method);
            return StatusCode(500, new { message = "Internal server error" });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid data", errors = ModelState });

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = InvalidUserTokenMessage });

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

                Log.Information("Task created successfully for UserId: {UserId}", userId?.ToString());
                return Ok(new { message = TaskCreatedMessage });
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(CreateTask));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = InvalidUserTokenMessage });

                var tasks = await _context.UserTasks
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.DueDate)
                    .ToListAsync();

                Log.Information("Fetched task list for UserId: {UserId}", userId?.ToString());
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(GetTasks));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = InvalidUserTokenMessage });

                var task = await _context.UserTasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    Log.Warning("Task not found. TaskId: {TaskId}, UserId: {UserId}", id, userId?.ToString());
                    return NotFound(new { message = TaskNotFoundMessage });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(GetTaskById));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] CreateTaskDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid data", errors = ModelState });

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = InvalidUserTokenMessage });

                var task = await _context.UserTasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                    return NotFound(new { message = TaskNotFoundMessage });

                task.Title = dto.Title;
                task.Description = dto.Description;
                task.DueDate = dto.DueDate;
                task.Status = dto.Status;
                task.Priority = dto.Priority;

                await _context.SaveChangesAsync();
                Log.Information("Task {TaskId} updated by UserId {UserId}", id, userId?.ToString());

                return Ok(new { message = TaskUpdatedMessage });
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(UpdateTask));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = InvalidUserTokenMessage });

                var task = await _context.UserTasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                    return NotFound(new { message = TaskNotFoundMessage });

                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();

                Log.Information("Task {TaskId} deleted by UserId {UserId}", id, userId?.ToString());
                return Ok(new { message = TaskDeletedMessage });
            }
            catch (Exception ex)
            {
                return HandleServerError(ex, nameof(DeleteTask));
            }
        }
    }
}