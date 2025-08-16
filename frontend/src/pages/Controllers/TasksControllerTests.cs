using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Backend.Tests.Controllers
{
    public class TasksControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private ClaimsPrincipal GetFakeUser(int userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        }

        [Fact]
        public async Task GetTasks_ReturnsUserSpecificTasks()
        {
            var context = GetInMemoryDbContext();

            context.UserTasks.Add(new UserTask
            {
                Title = "User1 Task",
                Description = "Test task",
                DueDate = DateTime.Now.AddDays(3),
                Status = "Pending",
                Priority = "High",
                UserId = 1
            });

            context.UserTasks.Add(new UserTask
            {
                Title = "User2 Task",
                Description = "Should not return",
                DueDate = DateTime.Now.AddDays(1),
                Status = "Completed",
                Priority = "Low",
                UserId = 2
            });

            await context.SaveChangesAsync();

            var controller = new TasksController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = GetFakeUser(1)
                }
            };

            var result = await controller.GetTasks();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<UserTask>>(okResult.Value);
            Assert.Single(tasks);
            Assert.Equal("User1 Task", tasks.First().Title);
        }

        [Fact]
        public async Task CreateTask_AddsTaskSuccessfully()
        {
            var context = GetInMemoryDbContext();
            var controller = new TasksController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = GetFakeUser(1)
                }
            };

            var dto = new Backend.DTO.CreateTaskDto
            {
                Title = "New Task",
                Description = "Desc",
                Status = "Pending",
                Priority = "High",
                DueDate = DateTime.Now.AddDays(2)
            };

            var result = await controller.CreateTask(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(1, context.UserTasks.Count());
            Assert.Equal("New Task", context.UserTasks.First().Title);
        }
    }
}