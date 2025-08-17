using Xunit;
using Microsoft.AspNetCore.Mvc;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class AdminTasksControllerTests
    {
        private AppDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique db per test
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllTasks_ReturnsOk()
        {
            using var ctx = GetDb();

            // Add a test user
            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Email = "test@user.com",
                Role = "User",
                IsActive = true
            };
            ctx.Users.Add(user);

            // Add a test task linked to user
            ctx.UserTasks.Add(new UserTask
            {
                Id = 1,
                Title = "Test Task",
                Status = "Pending",
                Priority = "Low",
                UserId = user.Id
            });

            ctx.SaveChanges();

            var controller = new AdminTasksController(ctx);
            var result = await controller.GetAllTasks();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTaskById_ReturnsOk_WhenFound()
        {
            using var ctx = GetDb();

            // Add a test user
            var user = new User
            {
                Id = 2,
                FullName = "Another User",
                Email = "another@user.com",
                Role = "User",
                IsActive = true
            };
            ctx.Users.Add(user);

            // Add a test task linked to user
            ctx.UserTasks.Add(new UserTask
            {
                Id = 2,
                Title = "Sample Task",
                Status = "Completed",
                Priority = "High",
                UserId = user.Id
            });

            ctx.SaveChanges();

            var controller = new AdminTasksController(ctx);
            var result = await controller.GetTaskById(2);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
