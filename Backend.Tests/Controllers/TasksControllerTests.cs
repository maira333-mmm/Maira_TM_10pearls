using Backend.Controllers;
using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Tests
{
    public class TasksControllerTests
    {
        [Fact]
        public async Task CreateTask_Unauthorized_WhenNoUser()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TasksController(ctx);
            var res = await sut.CreateTask(new CreateTaskDto());
            Assert.IsType<UnauthorizedObjectResult>(res);
        }

        [Fact]
        public async Task CreateTask_BadRequest_WhenModelInvalid()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(1)
            };
            sut.ModelState.AddModelError("Title", "Required");
            var res = await sut.CreateTask(new CreateTaskDto());
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        public async Task CreateTask_Ok_WhenValid()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TasksController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(2)
            };

            var dto = new CreateTaskDto
            {
                Title = "Task",
                Description = "D",
                DueDate = DateTime.UtcNow,
                Status = "Pending",
                Priority = "High"
            };

            var res = await sut.CreateTask(dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            Assert.Equal(200, ok.StatusCode);
            Assert.True(await ctx.UserTasks.AnyAsync(t => t.UserId == 2 && t.Title == "Task"));
        }

        [Fact]
        public async Task CreateTask_500_OnException()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            using var ctx = new ThrowingDbContext(options);
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(1) };

            var res = await sut.CreateTask(new CreateTaskDto
            {
                Title = "X", Description = "Y", DueDate = DateTime.UtcNow, Status = "Pending", Priority = "Low"
            });

            var obj = Assert.IsType<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetTasks_Unauthorized_WhenNoUser()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TasksController(ctx);
            var res = await sut.GetTasks();
            Assert.IsType<UnauthorizedObjectResult>(res);
        }

        [Fact]
        public async Task GetTasks_Ok_ReturnsOnlyUserTasks()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 1, 1, "A");
            TestHelpers.SeedTask(ctx, 2, 2, "B");

            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(1) };
            var res = await sut.GetTasks();
            var ok = Assert.IsType<OkObjectResult>(res);
            var list = Assert.IsAssignableFrom<IEnumerable<UserTask>>(ok.Value);
            Assert.All(list, t => Assert.Equal(1, t.UserId));
        }

        [Fact]
        public async Task GetTaskById_NotFound_WhenOtherUser()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 5, 2, "Z");
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(1) };
            var res = await sut.GetTaskById(5);
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task GetTaskById_Ok_WhenOwner()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 8, 3, "Mine");
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(3) };
            var res = await sut.GetTaskById(8);
            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public async Task UpdateTask_BadRequest_WhenModelInvalid()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(1) };
            sut.ModelState.AddModelError("Title", "Required");
            var res = await sut.UpdateTask(1, new CreateTaskDto());
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task UpdateTask_NotFound_WhenNotOwner()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 11, 2, "X");
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(1) };
            var res = await sut.UpdateTask(11, new CreateTaskDto
            {
                Title = "N", Description = "D", DueDate = DateTime.UtcNow, Status = "In Progress", Priority = "Low"
            });
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task UpdateTask_Ok_WhenOwner()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 12, 4, "Old");
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(4) };
            var res = await sut.UpdateTask(12, new CreateTaskDto
            {
                Title = "New", Description = "D", DueDate = DateTime.UtcNow, Status = "Completed", Priority = "High"
            });
            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public async Task DeleteTask_NotFound_WhenNotOwner()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 13, 2, "X");
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(1) };
            var res = await sut.DeleteTask(13);
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task DeleteTask_Ok_WhenOwner()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedTask(ctx, 14, 6, "Del");
            var sut = new TasksController(ctx) { ControllerContext = TestHelpers.CtxWithUser(6) };
            var res = await sut.DeleteTask(14);
            Assert.IsType<OkObjectResult>(res);
        }
    }
}
