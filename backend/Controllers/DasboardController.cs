using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Serilog;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : BaseDashboardController
    {
        public DashboardController(AppDbContext context) : base(context) { }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await GetCurrentUserSummaryAsync();
                if (summary == null)
                {
                    return Unauthorized(new { message = "Invalid or unauthorized user" });
                }

                Log.Information("Summary fetched successfully for normal user.");
                return Ok(summary);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching dashboard summary");
                return StatusCode(500, new { message = "An error occurred while fetching summary" });
            }
        }
    }
}
