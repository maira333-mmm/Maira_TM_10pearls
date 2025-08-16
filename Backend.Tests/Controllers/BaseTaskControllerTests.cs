using Backend.Controllers;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests
{
    // Public shim to expose protected members for direct coverage
    public class TestableBaseTaskController : BaseTaskController
    {
        public TestableBaseTaskController(AppDbContext ctx) : base(ctx) { }

        public int? ExposeGetCurrentUserId() => GetCurrentUserId();
        public IActionResult ExposeHandleServerError(Exception ex) => HandleServerError(ex, "TestMethod");
    }

    public class BaseTaskControllerTests
    {
        [Fact]
        public void GetCurrentUserId_ReturnsUserId_WhenClaimPresent()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TestableBaseTaskController(ctx)
            {
                ControllerContext = TestHelpers.CtxWithUser(userId: 42)
            };
            Assert.Equal(42, sut.ExposeGetCurrentUserId());
        }

        [Fact]
        public void GetCurrentUserId_ReturnsNull_WhenNoClaim()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TestableBaseTaskController(ctx);
            Assert.Null(sut.ExposeGetCurrentUserId());
        }

        [Fact]
        public void HandleServerError_Returns500()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TestableBaseTaskController(ctx);
            var res = sut.ExposeHandleServerError(new InvalidOperationException("x"));
            var obj = Assert.IsType<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
