using Backend.Controllers;
using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Backend.Tests
{
    public class AdminTasksControllerTests
    {
        [Fact]
        public async Task GetTaskById_NotFound_WhenMissing()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1, "a@a.com", role: "Admin")
            };
            var res = await sut.GetTaskById(999);
            Assert.Equal(404, ((ObjectResult)res).StatusCode);
        }

        [Fact]
        public async Task GetTaskById_Ok_WhenExists()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 2, "u@u.com", "U");
            TestHelpers.SeedTask(ctx, 5, 2, "Title");

            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(100, "admin@x.com", role: "Admin")
            };

            var res = await sut.GetTaskById(5);
            Assert.Equal(200, ((ObjectResult)res).StatusCode);
        }

        [Fact]
        public async Task GetAllTasks_Ok_ReturnsList()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 1, "a@a.com", "A");
            TestHelpers.SeedTask(ctx, 1, 1, "T1");
            TestHelpers.SeedTask(ctx, 2, 1, "T2");

            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            var res = await sut.GetAllTasks();
            Assert.Equal(200, ((ObjectResult)res).StatusCode);
        }

        [Fact]
        public async Task UpdateTaskByAdmin_BadRequest_WhenModelInvalid()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            sut.ModelState.AddModelError("Title", "Required");
            var res = await sut.UpdateTaskByAdmin(1, new CreateTaskDto());
            Assert.Equal(400, ((ObjectResult)res).StatusCode);
        }

        [Fact]
        public async Task UpdateTaskByAdmin_NotFound_WhenMissing()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            var res = await sut.UpdateTaskByAdmin(123, new CreateTaskDto
            {
                Title = "T", Description = "D", Status = "Pending", Priority = "Low", DueDate = DateTime.UtcNow
            });

            Assert.Equal(404, ((ObjectResult)res).StatusCode);
        }

        [Fact]
        public async Task UpdateTaskByAdmin_Ok_WhenExists()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 44, 1, "Old");

            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            var res = await sut.UpdateTaskByAdmin(44, new CreateTaskDto
            {
                Title = "New", Description = "D", Status = "In Progress", Priority = "High", DueDate = DateTime.UtcNow
            });

            Assert.Equal(200, ((ObjectResult)res).StatusCode);

            var updated = await ctx.UserTasks.FirstAsync(t => t.Id == 44);
            Assert.Equal("New", updated.Title);
        }

        [Fact]
        public async Task DeleteTask_NotFound_WhenMissing()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            var res = await sut.DeleteTask(9999);
            Assert.Equal(404, ((ObjectResult)res).StatusCode);
        }

        [Fact]
        public async Task DeleteTask_Ok_WhenExists()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 77, 1, "Del");

            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            var res = await sut.DeleteTask(77);
            Assert.Equal(200, ((ObjectResult)res).StatusCode);
            Assert.False(ctx.UserTasks.Any(t => t.Id == 77));
        }

        [Fact]
        public async Task AdminActions_Return500_OnException()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;

            using var ctx = new ThrowingDbContext(options);

            var sut = new AdminTasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(10, "admin@x.com", role: "Admin")
            };

            var res = await sut.UpdateTaskByAdmin(1, new CreateTaskDto
            {
                Title = "T", Description = "D", Status = "Pending", Priority = "Low", DueDate = DateTime.UtcNow
            });

            var obj = Assert.IsAssignableFrom<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
