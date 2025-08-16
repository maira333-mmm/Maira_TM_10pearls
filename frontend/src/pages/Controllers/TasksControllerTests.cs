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
        // ✅ Creates an in-memory DbContext (no actual SQL Server needed)
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            return new AppDbContext(options);
        }

        // ✅ Mocks a ClaimsPrincipal with user ID
        private ClaimsPrincipal GetFakeUser(int userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        }

        // ✅ Actual test method for GetTasks()
        [Fact]
        public async Task GetTasks_ReturnsUserSpecificTasks()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            // Add fake tasks for user 1 and user 2
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

            // Create controller and attach fake user with ID 1
            var controller = new TasksController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = GetFakeUser(1)
                    }
                }
            };

            // Act
            var result = await controller.GetTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<UserTask>>(okResult.Value);

            Assert.Single(tasks); // Only one task should be returned (user 1's task)
            Assert.Equal("User1 Task", tasks.First().Title);
        }
    }
}