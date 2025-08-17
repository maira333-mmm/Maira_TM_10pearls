using Backend.Controllers;
using Backend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
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
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "42")
                        }))
                    }
                }
            };

            Assert.Equal(42, sut.ExposeGetCurrentUserId());
        }

        [Fact]
        public void GetCurrentUserId_ReturnsNull_WhenNoClaim()
        {
            using var ctx = TestHelpers.NewDb();

            var sut = new TestableBaseTaskController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() // No claims here
                }
            };

            Assert.Null(sut.ExposeGetCurrentUserId());
        }

        [Fact]
        public void HandleServerError_Returns500()
        {
            using var ctx = TestHelpers.NewDb();
            var sut = new TestableBaseTaskController(ctx);

            var ex = new InvalidOperationException("Test exception");
            var res = sut.ExposeHandleServerError(ex);

            var obj = Assert.IsType<ObjectResult>(res);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
