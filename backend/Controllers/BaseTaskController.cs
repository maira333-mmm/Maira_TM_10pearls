using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Data;
using Serilog;

namespace Backend.Controllers
{
    [ApiController]
    public abstract class BaseTaskController : ControllerBase
    {
        protected readonly AppDbContext _context;

        protected BaseTaskController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get current userId from Claims
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        }

        // ✅ Standard error response
        protected IActionResult HandleServerError(Exception ex, string actionName)
        {
            Log.Error(ex, $"{actionName} Error");
            return StatusCode(500, new { message = "Server error", error = ex.Message });
        }
    }
}
