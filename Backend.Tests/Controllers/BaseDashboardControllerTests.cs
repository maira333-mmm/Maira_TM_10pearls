using Backend.Controllers;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests
{
    // Concrete public shim over abstract BaseDashboardController to expose protected methods
    public class TestableBaseDashboardController : BaseDashboardController
    {
        public TestableBaseDashboardController(AppDbContext context) : base(context) { }
        public Task<object?> ExposeGetUserSummaryAsync(int userId) => GetUserSummaryAsync(userId);
        public Task<object?> ExposeGetCurrentUserSummaryAsync() => GetCurrentUserSummaryAsync();
    }

    public class BaseDashboardControllerTests
    {
        [Fact]
        public async Task GetUserSummaryAsync_ReturnsCounts()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 1, "a@a.com", "A");
            TestHelpers.SeedTask(ctx, 1, 1, status: "Completed");
            TestHelpers.SeedTask(ctx, 2, 1, status: "In Progress");
            TestHelpers.SeedTask(ctx, 3, 1, status: "Pending");

            var sut = new TestableBaseDashboardController(ctx);
            var result = await sut.ExposeGetUserSummaryAsync(1);
            Assert.NotNull(result);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("completed", json);
            Assert.Contains("inProgress", json);
            Assert.Contains("pending", json);
        }

        [Fact]
        public async Task GetCurrentUserSummaryAsync_ReturnsNull_WhenNoEmailClaim()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TestableBaseDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 1, email: null!)
            };
            var res = await sut.ExposeGetCurrentUserSummaryAsync();
            Assert.Null(res);
        }

        [Fact]
        public async Task GetCurrentUserSummaryAsync_ReturnsSummary_WhenEmailMatches()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 7, "user@x.com", "U");
            TestHelpers.SeedTask(ctx, 10, 7, status: "Completed");
            var sut = new TestableBaseDashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 7, email: "user@x.com")
            };
            var res = await sut.ExposeGetCurrentUserSummaryAsync();
            Assert.NotNull(res);
        }
    }
}
