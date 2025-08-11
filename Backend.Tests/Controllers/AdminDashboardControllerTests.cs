using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Text.Json;

namespace Backend.Tests.Controllers
{
    public class AdminDashboardControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public AdminDashboardControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ClaimsPrincipal GetAdminClaimsPrincipal()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        private AdminDashboardController CreateController(AppDbContext context)
        {
            return new AdminDashboardController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = GetAdminClaimsPrincipal()
                    }
                }
            };
        }

        [Fact]
        public async Task GetAllUsers_Returns_OnlyActiveNonAdmins()
        {
            using var context = new AppDbContext(_dbOptions);
            context.Users.AddRange(
                new User { Id = 1, FullName = "User1", Email = "u1@test.com", Role = "User", IsActive = true },
                new User { Id = 2, FullName = "Admin", Email = "admin@test.com", Role = "Admin", IsActive = true },
                new User { Id = 3, FullName = "InactiveUser", Email = "u2@test.com", Role = "User", IsActive = false }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetAllUsers();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Single(users);
        }

        [Fact]
        public async Task GetAdminSummary_Returns_CorrectData()
        {
            using var context = new AppDbContext(_dbOptions);
            var user = new User
            {
                Id = 1,
                FullName = "User1",
                Email = "u1@test.com",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.UserTasks.AddRange(
                new UserTask { Title = "Task1", Status = "Completed", UserId = 1 },
                new UserTask { Title = "Task2", Status = "Pending", UserId = 1 },
                new UserTask { Title = "Task3", Status = "In Progress", UserId = 1 }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.GetAdminSummary();
            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(1, root.GetProperty("taskStats").GetProperty("completed").GetInt32());
            Assert.Equal(1, root.GetProperty("taskStats").GetProperty("pending").GetInt32());
            Assert.Equal(1, root.GetProperty("taskStats").GetProperty("inProgress").GetInt32());
            Assert.Equal(1, root.GetProperty("userStats").GetProperty("total").GetInt32());
            Assert.Equal(1, root.GetProperty("userStats").GetProperty("active").GetInt32());
        }

        [Fact]
        public async Task ToggleUserActivation_Changes_Status()
        {
            using var context = new AppDbContext(_dbOptions);
            context.Users.Add(new User { Id = 1, FullName = "User1", Role = "User", IsActive = true });
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.ToggleUserActivation(1);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);
            bool isActive = doc.RootElement.GetProperty("isActive").GetBoolean();

            Assert.False(isActive);
        }

        [Fact]
        public async Task ToggleUserActivation_Returns_NotFound_IfUserMissing()
        {
            using var context = new AppDbContext(_dbOptions);
            var controller = CreateController(context);

            var result = await controller.ToggleUserActivation(99);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTask_Removes_Task()
        {
            using var context = new AppDbContext(_dbOptions);
            context.UserTasks.Add(new UserTask { Id = 1, Title = "Test", UserId = 1 });
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.DeleteTask(1);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.UserTasks);
        }

        [Fact]
        public async Task GetTaskById_Returns_TaskDetails()
        {
            using var context = new AppDbContext(_dbOptions);
            var user = new User { Id = 1, FullName = "User1" };
            var task = new UserTask
            {
                Id = 1,
                Title = "Task1",
                Description = "Desc",
                Status = "Pending",
                Priority = "High",
                UserId = 1,
                User = user
            };
            context.Users.Add(user);
            context.UserTasks.Add(task);
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.GetTaskById(1);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Case-insensitive lookup
            string title = root.TryGetProperty("title", out var lowerTitle)
                ? lowerTitle.GetString()
                : root.TryGetProperty("Title", out var upperTitle)
                    ? upperTitle.GetString()
                    : null;

            Assert.Equal("Task1", title);
        }

        [Fact]
        public async Task CreateTask_Assigns_Task_To_ValidUser()
        {
            using var context = new AppDbContext(_dbOptions);
            var user = new User { Id = 1, FullName = "User1", Role = "User", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var dto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "Task Desc",
                DueDate = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                Priority = "Medium",
                UserId = 1
            };

            var result = await controller.CreateTask(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Single(context.UserTasks);
        }

        [Fact]
        public async Task CreateTask_Returns_BadRequest_For_InvalidUser()
        {
            using var context = new AppDbContext(_dbOptions);
            var controller = CreateController(context);

            var dto = new CreateTaskDto
            {
                Title = "Invalid Task",
                UserId = 99,
                Status = "Pending",
                Priority = "Low",
                DueDate = DateTime.UtcNow
            };

            var result = await controller.CreateTask(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
