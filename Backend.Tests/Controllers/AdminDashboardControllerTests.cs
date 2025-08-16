using Backend.Controllers;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Tests
{
    public class AdminDashboardControllerTests
    {
        [Fact]
        public async Task GetAllUsers_Ok_ReturnsOnlyActiveNonAdmins()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 1, "a@a.com", "Admin", role: "Admin");
            TestHelpers.SeedUser(ctx, 2, "u1@x.com", "U1", role: "User", isActive: true);
            TestHelpers.SeedUser(ctx, 3, "u2@x.com", "U2", role: "User", isActive: false);

            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 1, email: "a@a.com", role: "Admin")
            };

            var res = await sut.GetAllUsers();
            var ok = Assert.IsType<OkObjectResult>(res);
            var list = ((IEnumerable<object>)ok.Value!).ToList();
            Assert.Single(list); // only U1
        }

        [Fact]
        public async Task GetAdminSummary_Ok_ReturnsStats()
        {
            using var ctx = TestHelpers.NewDb();
            // users
            TestHelpers.SeedUser(ctx, 1, "a@a.com", "Admin", role: "Admin");
            TestHelpers.SeedUser(ctx, 2, "u1@x.com", "U1", role: "User", isActive: true, createdAt: DateTime.UtcNow.AddDays(-1));
            TestHelpers.SeedUser(ctx, 3, "u2@x.com", "U2", role: "User", isActive: true, createdAt: DateTime.UtcNow.AddDays(-10));
            // tasks
            TestHelpers.SeedTask(ctx, 10, 2, status: "Completed");
            TestHelpers.SeedTask(ctx, 11, 2, status: "Pending");
            TestHelpers.SeedTask(ctx, 12, 3, status: "In Progress");

            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 1, email: "a@a.com", role: "Admin")
            };

            var res = await sut.GetAdminSummary();
            var ok = Assert.IsType<OkObjectResult>(res);
            var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            Assert.Contains("taskStats", json);
            Assert.Contains("userStats", json);
            Assert.Contains("recentTasks", json);
            Assert.Contains("users", json);
        }

        [Fact]
        public async Task ToggleUserActivation_NotFound_WhenMissing()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var res = await sut.ToggleUserActivation(999);
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task ToggleUserActivation_BadRequest_ForAdminUser()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 1, "admin@x.com", "Admin", role: "Admin");
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var res = await sut.ToggleUserActivation(1);
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task ToggleUserActivation_Ok_TogglesFlag()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 2, "u@x.com", "U", role: "User", isActive: true);
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var res = await sut.ToggleUserActivation(2);
            var ok = Assert.IsType<OkObjectResult>(res);
            Assert.False((await ctx.Users.FindAsync(2))!.IsActive);
        }

        [Fact]
        public async Task DeleteTask_NotFound_WhenMissing()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var res = await sut.DeleteTask(999);
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task DeleteTask_Ok_WhenExists()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 50, 2, "Del");
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var res = await sut.DeleteTask(50);
            Assert.IsType<OkObjectResult>(res);
            Assert.False(ctx.UserTasks.Any(t => t.Id == 50));
        }

        [Fact]
        public async Task GetTaskById_Ok_And_NotFound()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 60, 2, "View");
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var ok = await sut.GetTaskById(60);
            Assert.IsType<OkObjectResult>(ok);

            var nf = await sut.GetTaskById(999);
            Assert.IsType<NotFoundObjectResult>(nf);
        }

        [Fact]
        public async Task CreateTask_BadRequest_ForInvalidUserOrInactiveOrAdmin()
        {
            using var ctx = TestHelpers.NewDb();
            // user missing
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var bad1 = await sut.CreateTask(new Backend.DTO.CreateTaskDto
            {
                UserId = 999, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow
            });
            Assert.IsType<BadRequestObjectResult>(bad1);

            // admin user
            TestHelpers.SeedUser(ctx, 10, "adm@x.com", "A", role: "Admin");
            var bad2 = await sut.CreateTask(new Backend.DTO.CreateTaskDto
            {
                UserId = 10, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow
            });
            Assert.IsType<BadRequestObjectResult>(bad2);

            // inactive user
            TestHelpers.SeedUser(ctx, 11, "ina@x.com", "I", role: "User", isActive: false);
            var bad3 = await sut.CreateTask(new Backend.DTO.CreateTaskDto
            {
                UserId = 11, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow
            });
            Assert.IsType<BadRequestObjectResult>(bad3);
        }

        [Fact]
        public async Task CreateTask_Ok_ForValidUser()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 20, "u@x.com", "U", role: "User", isActive: true);
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var ok = await sut.CreateTask(new Backend.DTO.CreateTaskDto
            {
                UserId = 20, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow
            });
            Assert.IsType<OkObjectResult>(ok);
            Assert.True(ctx.UserTasks.Any(t => t.UserId == 20));
        }

        [Fact]
        public async Task AdminDashboard_Endpoints_Return500_OnException()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            using var ctx = new ThrowingDbContext(options);
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };
            var res = await sut.GetAllUsers();
            var obj = Assert.IsType<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
