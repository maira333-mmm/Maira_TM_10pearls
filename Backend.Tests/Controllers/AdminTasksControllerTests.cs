using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers
{
    public class AdminTasksControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public AdminTasksControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ClaimsPrincipal GetAdminUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        private AdminTasksController CreateController(AppDbContext context)
        {
            return new AdminTasksController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = GetAdminUser()
                    }
                }
            };
        }

        [Fact]
        public async Task GetTaskById_Returns_TaskDetails()
        {
            using var context = new AppDbContext(_dbOptions);
            var user = new User { Id = 1, FullName = "Test User" };
            var task = new UserTask
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Desc",
                Status = "Pending",
                Priority = "High",
                DueDate = DateTime.UtcNow,
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

            Assert.Equal("Test Task", doc.RootElement.GetProperty("Title").GetString());
            Assert.Equal("Test User", doc.RootElement.GetProperty("AssignedTo").GetString());
        }

        [Fact]
        public async Task GetTaskById_Returns_NotFound_When_TaskDoesNotExist()
        {
            using var context = new AppDbContext(_dbOptions);
            var controller = CreateController(context);

            var result = await controller.GetTaskById(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Task not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task UpdateTaskByAdmin_Updates_Task_Successfully()
        {
            using var context = new AppDbContext(_dbOptions);
            var task = new UserTask
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Desc",
                Status = "Pending",
                Priority = "Low",
                DueDate = DateTime.UtcNow
            };
            context.UserTasks.Add(task);
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var dto = new CreateTaskDto
            {
                Title = "New Title",
                Description = "New Desc",
                Status = "Completed",
                Priority = "High",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var result = await controller.UpdateTaskByAdmin(1, dto);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);

            Assert.Equal("New Title", doc.RootElement.GetProperty("task").GetProperty("Title").GetString());
        }

        [Fact]
        public async Task UpdateTaskByAdmin_Returns_NotFound_When_TaskMissing()
        {
            using var context = new AppDbContext(_dbOptions);
            var controller = CreateController(context);

            var dto = new CreateTaskDto
            {
                Title = "Doesn't Matter",
                Description = "NA",
                Status = "Pending",
                Priority = "Low",
                DueDate = DateTime.UtcNow
            };

            var result = await controller.UpdateTaskByAdmin(99, dto);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetAllTasks_Returns_ListOfTasks()
        {
            using var context = new AppDbContext(_dbOptions);
            context.Users.Add(new User { Id = 1, FullName = "Test User" });
            context.UserTasks.Add(new UserTask
            {
                Id = 1,
                Title = "Task 1",
                Description = "Desc",
                Status = "Pending",
                Priority = "Low",
                UserId = 1
            });
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.GetAllTasks();
            var okResult = Assert.IsType<OkObjectResult>(result);

            var tasks = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(tasks);
        }

        [Fact]
        public async Task DeleteTask_Removes_Task_Successfully()
        {
            using var context = new AppDbContext(_dbOptions);
            context.UserTasks.Add(new UserTask { Id = 1, Title = "To Delete" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.DeleteTask(1);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.UserTasks);
        }

        [Fact]
        public async Task DeleteTask_Returns_NotFound_When_TaskMissing()
        {
            using var context = new AppDbContext(_dbOptions);
            var controller = CreateController(context);

            var result = await controller.DeleteTask(99);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}