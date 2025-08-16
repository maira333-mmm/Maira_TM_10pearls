using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTO;
using Serilog;

namespace Backend.Controllers
{
    [Route("api/admin/tasks")]
    [Authorize(Roles = "Admin")]
    public class AdminTasksController : BaseTaskController
    {
        public AdminTasksController(AppDbContext context) : base(context) { }

        // GET: api/admin/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var task = await _context.UserTasks
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                    return NotFound(new { message = "Task not found or unauthorized" });

                return Ok(new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    task.DueDate,
                    task.Status,
                    task.Priority,
                    AssignedTo = task.User != null ? task.User.FullName : "Unassigned"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {ActionName}", nameof(GetTaskById));
                return StatusCode(500, new { message = "Server error occurred" });
            }
        }

        // GET: api/admin/tasks
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
                        AssignedTo = t.User != null ? t.User.FullName : "Unassigned"
                    })
                    .ToListAsync();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {ActionName}", nameof(GetAllTasks));
                return StatusCode(500, new { message = "Server error occurred" });
            }
        }

        // PUT: api/admin/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskByAdmin(int id, [FromBody] CreateTaskDto dto)
        {
            try
            {
                if (dto == null || !ModelState.IsValid)
                    return BadRequest(new { message = "Invalid data", errors = ModelState });

                var task = await _context.UserTasks.FindAsync(id);
                if (task == null)
                    return NotFound(new { message = "Task not found or unauthorized" });

                task.Title = dto.Title;
                task.Description = dto.Description;
                task.DueDate = dto.DueDate;
                task.Status = dto.Status;
                task.Priority = dto.Priority;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Task updated successfully by admin!", task });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {ActionName}", nameof(UpdateTaskByAdmin));
                return StatusCode(500, new { message = "Server error occurred" });
            }
        }

        // DELETE: api/admin/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.UserTasks.FindAsync(id);
                if (task == null)
                    return NotFound(new { message = "Task not found" });

                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {ActionName}", nameof(DeleteTask));
                return StatusCode(500, new { message = "Server error occurred" });
            }
        }
    }
}