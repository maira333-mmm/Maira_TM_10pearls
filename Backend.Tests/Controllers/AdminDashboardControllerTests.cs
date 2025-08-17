using Xunit;
using Microsoft.AspNetCore.Mvc;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class AdminDashboardControllerTests
    {
        private AppDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("AdminDashboardTestDb")
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk()
        {
            using var ctx = GetDb();
            ctx.Users.Add(new User { Id = 1, FullName = "Test User", Email = "test@test.com", Role = "User", IsActive = true });
            ctx.SaveChanges();

            var controller = new AdminDashboardController(ctx);
            var result = await controller.GetAllUsers();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAdminSummary_ReturnsOk()
        {
            using var ctx = GetDb();
            var controller = new AdminDashboardController(ctx);
            var result = await controller.GetAdminSummary();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
