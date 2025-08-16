using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Serilog;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/tasks")]
    [Authorize(Roles = "Admin")] // ✅ Only accessible by Admins
    public class AdminTasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminTasksController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/admin/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var task = await _context.UserTasks
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    Log.Warning("Admin attempted to fetch non-existing task. TaskId: {TaskId}", id);
                    return NotFound(new { message = "Task not found or unauthorized" });
                }

                Log.Information("Admin fetched task details. TaskId: {TaskId}", id);

                return Ok(new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    task.DueDate,
                    task.Status,
                    task.Priority,
                    AssignedTo = task.User?.FullName ?? "Unassigned"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching task details. TaskId: {TaskId}", id);
                return StatusCode(500, new { message = "Error retrieving task", error = ex.Message });
            }
        }

        // ✅ PUT: api/admin/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskByAdmin(int id, [FromBody] CreateTaskDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid model state while updating task. TaskId: {TaskId}", id);
                    return BadRequest(new { message = "Invalid data", errors = ModelState });
                }

                var task = await _context.UserTasks.FindAsync(id);
                if (task == null)
                {
                    Log.Warning("Admin attempted to update non-existing task. TaskId: {TaskId}", id);
                    return NotFound(new { message = "Task not found or unauthorized" });
                }

                // Optionally log previous values for audit
                Log.Information("Admin updating task. TaskId: {TaskId}, OldTitle: {OldTitle}", id, task.Title);

                task.Title = dto.Title;
                task.Description = dto.Description;
                task.DueDate = dto.DueDate;
                task.Status = dto.Status;
                task.Priority = dto.Priority;

                await _context.SaveChangesAsync();

                Log.Information("Admin updated task successfully. TaskId: {TaskId}, NewTitle: {NewTitle}", id, task.Title);

                return Ok(new
                {
                    message = "Task updated successfully by admin!",
                    task = new
                    {
                        task.Id,
                        task.Title,
                        task.Description,
                        task.DueDate,
                        task.Status,
                        task.Priority,
                        task.UserId
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating task. TaskId: {TaskId}", id);
                return StatusCode(500, new { message = "Error updating task", error = ex.Message });
            }
        }

        // ✅ GET: api/admin/tasks
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            try
            {
                var tasks = await _context.UserTasks
                    .Include(t => t.User)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.DueDate,
                        t.Status,
                        t.Priority,
                        AssignedTo = t.User.FullName
                    })
                    .ToListAsync();

                Log.Information("Admin fetched all tasks. Count: {Count}", tasks.Count);

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all tasks for admin");
                return StatusCode(500, new { message = "Error retrieving tasks", error = ex.Message });
            }
        }

        // ✅ DELETE: api/admin/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.UserTasks.FindAsync(id);
                if (task == null)
                {
                    Log.Warning("Admin attempted to delete non-existing task. TaskId: {TaskId}", id);
                    return NotFound(new { message = "Task not found" });
                }

                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();

                Log.Information("Admin deleted task. TaskId: {TaskId}", id);

                return Ok(new { message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting task. TaskId: {TaskId}", id);
                return StatusCode(500, new { message = "Error deleting task", error = ex.Message });
            }
        }
    }
}
