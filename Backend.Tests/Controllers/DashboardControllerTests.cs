using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Backend.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public DashboardControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "DashboardDb")
                .Options;
        }

        private ClaimsPrincipal GetClaimsPrincipal(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task GetSummary_Returns_Correct_Summary()
        {
            using var context = new AppDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed test user
            var testUser = new User
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com"
            };
            context.Users.Add(testUser);

            // Seed test tasks
            context.UserTasks.AddRange(
                new UserTask { UserId = 1, Status = "Completed" },
                new UserTask { UserId = 1, Status = "In Progress" },
                new UserTask { UserId = 1, Status = "Pending" }
            );

            await context.SaveChangesAsync();

            // Mock logger
            var logger = new Mock<ILogger<DashboardController>>();

            // Create controller
            var controller = new DashboardController(context, logger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = GetClaimsPrincipal("john@example.com")
                    }
                }
            };

            // Act
            var result = await controller.GetSummary();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check task counts
            Assert.Equal(1, root.GetProperty("completed").GetInt32());
            Assert.Equal(1, root.GetProperty("inProgress").GetInt32());
            Assert.Equal(1, root.GetProperty("pending").GetInt32());

            // Check user info
            var user = root.GetProperty("user");
            Assert.Equal("John Doe", user.GetProperty("fullName").GetString());
            Assert.Equal("john@example.com", user.GetProperty("email").GetString());
        }
    }
}