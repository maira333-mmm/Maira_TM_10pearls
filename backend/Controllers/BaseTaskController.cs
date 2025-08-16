using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using System.Security.Claims;
using Serilog;

namespace Backend.Controllers
{
    public class BaseTaskController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public BaseTaskController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Extract UserId from JWT token claims
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "id");
            if (userIdClaim == null) return null;

            if (int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            return null;
        }

        protected IActionResult HandleServerError(Exception ex, string methodName)
        {
            // Fixed: Use Serilog message template instead of string interpolation
            Log.Error(ex, "Error in {MethodName}", methodName);
            return StatusCode(500, new { message = "An internal server error occurred" });
        }
    }
}
