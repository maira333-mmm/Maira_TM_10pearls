using Backend.Controllers;
using Backend.Data;
using Backend.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var ok = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(200, ok.StatusCode);

            var list = ((IEnumerable<object>)ok.Value!).ToList();
            Assert.Single(list); // only U1
        }

        [Fact]
        public async Task GetAdminSummary_Ok_ReturnsStats()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 1, "a@a.com", "Admin", role: "Admin");
            TestHelpers.SeedUser(ctx, 2, "u1@x.com", "U1", role: "User", isActive: true, createdAt: DateTime.UtcNow.AddDays(-1));
            TestHelpers.SeedUser(ctx, 3, "u2@x.com", "U2", role: "User", isActive: true, createdAt: DateTime.UtcNow.AddDays(-10));
            TestHelpers.SeedTask(ctx, 10, 2, status: "Completed");
            TestHelpers.SeedTask(ctx, 11, 2, status: "Pending");
            TestHelpers.SeedTask(ctx, 12, 3, status: "In Progress");

            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 1, email: "a@a.com", role: "Admin")
            };

            var res = await sut.GetAdminSummary();
            var ok = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(200, ok.StatusCode);

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
            var nf = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(404, nf.StatusCode);
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
            var bad = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(400, bad.StatusCode);
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
            var ok = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(200, ok.StatusCode);

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
            var nf = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(404, nf.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_Ok_WhenExists()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 2, "u@x.com", "U");
            TestHelpers.SeedTask(ctx, 50, 2, "Del");

            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };

            var res = await sut.DeleteTask(50);
            var ok = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(200, ok.StatusCode);

            Assert.False(ctx.UserTasks.Any(t => t.Id == 50));
        }

        [Fact]
        public async Task GetTaskById_Ok_And_NotFound()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 2, "u@x.com", "U");
            TestHelpers.SeedTask(ctx, 60, 2, "View");

            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };

            var okRes = await sut.GetTaskById(60);
            var ok = Assert.IsAssignableFrom<ObjectResult>(okRes);
            Assert.Equal(200, ok.StatusCode);

            var nfRes = await sut.GetTaskById(999);
            var nf = Assert.IsAssignableFrom<ObjectResult>(nfRes);
            Assert.Equal(404, nf.StatusCode);
        }

        [Fact]
        public async Task CreateTask_BadRequest_ForInvalidUserOrInactiveOrAdmin()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };

            var bad1 = await sut.CreateTask(new CreateTaskDto { UserId = 999, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow });
            Assert.Equal(400, ((ObjectResult)bad1).StatusCode);

            TestHelpers.SeedUser(ctx, 10, "adm@x.com", "A", role: "Admin");
            var bad2 = await sut.CreateTask(new CreateTaskDto { UserId = 10, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow });
            Assert.Equal(400, ((ObjectResult)bad2).StatusCode);

            TestHelpers.SeedUser(ctx, 11, "ina@x.com", "I", role: "User", isActive: false);
            var bad3 = await sut.CreateTask(new CreateTaskDto { UserId = 11, Title = "T", Description = "D", Priority = "Low", Status = "Pending", DueDate = DateTime.UtcNow });
            Assert.Equal(400, ((ObjectResult)bad3).StatusCode);
        }

        [Fact]
        public async Task CreateTask_Ok_ForValidUser()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 20, "u@x.com", "U");

            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };

            var dto = new CreateTaskDto { UserId = 20, Title = "New Task", Description = "D", Status = "Pending", Priority = "High", DueDate = DateTime.UtcNow };
            var res = await sut.CreateTask(dto);
            var ok = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(200, ok.StatusCode);

            Assert.True(ctx.UserTasks.Any(t => t.UserId == 20 && t.Title == "New Task"));
        }

        [Fact]
        public async Task AdminDashboard_Endpoints_Return500_OnException()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("throw500").Options;

            using var ctx = new ThrowingDbContext(options);
            var sut = new AdminDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "admin@x.com", role: "Admin")
            };

            var res = await sut.ToggleUserActivation(999);
            var obj = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
