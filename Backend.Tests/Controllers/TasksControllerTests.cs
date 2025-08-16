using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers
{
    public class TasksControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private ClaimsPrincipal GetFakeUser(int userId)
{
    return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
    {
        new Claim("id", userId.ToString()) // match GetCurrentUserId()
    }, "mock"));
}


        [Fact]
        public async Task CreateTask_Creates_TaskSuccessfully()
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

            var dto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "Task Desc",
                Status = "Pending",
                Priority = "High",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var result = await controller.CreateTask(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Single(context.UserTasks);
            Assert.Equal("New Task", context.UserTasks.First().Title);
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
    }
}