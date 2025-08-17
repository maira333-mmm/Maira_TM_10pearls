using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Backend.Tests
{
    public static class TestHelpers
    {
        public static AppDbContext NewDb(string? name = null)
        {
            var dbName = name ?? Guid.NewGuid().ToString("N");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options;
            var ctx = new AppDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        public static ControllerContext CtxWithUser(
            int? userId = null, string? email = null, string role = "User",
            bool includeAltIdClaim = false)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
                if (includeAltIdClaim)
                    claims.Add(new Claim("id", userId.Value.ToString()));
            }
            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new Claim(ClaimTypes.Name, email));
            claims.Add(new Claim(ClaimTypes.Role, role));
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            return new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        public static void SeedUser(AppDbContext ctx, int id, string email, string fullName, string role = "User", bool isActive = true, DateTime? createdAt = null)
        {
            ctx.Users.Add(new User
            {
                Id = id,
                Email = email,
                FullName = fullName,
                Role = role,
                IsActive = isActive,
                CreatedAt = createdAt ?? DateTime.UtcNow
            });
            ctx.SaveChanges();
        }

        public static void SeedTask(AppDbContext ctx, int id, int userId, string title = "T", string status = "Pending", string priority = "Low", DateTime? due = null)
        {
            ctx.UserTasks.Add(new UserTask
            {
                Id = id,
                UserId = userId,
                Title = title,
                Description = "D",
                Status = status,
                Priority = priority,
                DueDate = due ?? DateTime.UtcNow
            });
            ctx.SaveChanges();
        }
    }

    // Forces SaveChangesAsync to throw to test 500 responses
    public class ThrowingDbContext : AppDbContext
    {
        public ThrowingDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public override int SaveChanges()
            => throw new InvalidOperationException("boom");
    }
}