using Backend.Controllers;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests
{
    public class DashboardControllerTests
    {
        [Fact]
        public async Task GetSummary_Unauthorized_WhenNoEmailClaim()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new DashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 1, email: null!)
            };
            var res = await sut.GetSummary();
            Assert.IsType<UnauthorizedObjectResult>(res);
        }

        [Fact]
        public async Task GetSummary_Ok_WhenValidUser()
        {
            using var ctx = TestHelpers.NewDb();
            TestHelpers.SeedUser(ctx, 9, "u@u.com", "User Nine");
            TestHelpers.SeedTask(ctx, 90, 9, status: "Completed");
            var sut = new DashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 9, email: "u@u.com")
            };
            var res = await sut.GetSummary();
            var ok = Assert.IsType<OkObjectResult>(res);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetSummary_Returns500_OnException()
        {
            // Dispose context to force ObjectDisposedException when enumerating
            var ctx = TestHelpers.NewDb();
            var sut = new DashboardController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 1, email: "x@x.com")
            };
            ctx.Dispose();

            var res = await sut.GetSummary();
            var obj = Assert.IsType<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
