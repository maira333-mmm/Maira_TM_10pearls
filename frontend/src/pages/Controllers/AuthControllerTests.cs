using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System;

namespace Backend.Tests
{
    public class AuthControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration GetTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "this_is_a_very_strong_secret_key_123456"},
                {"Jwt:Issuer", "test.issuer.com"},
                {"Jwt:Audience", "test.audience.com"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        private string ComputeSha256Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        [Fact]
        public async Task Signup_Should_Create_User_When_Email_Is_Unique()
        {
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);

            var newUser = new User
            {
                Email = "test@example.com",
                PasswordHash = "password123",
                FullName = "Test User",
                Role = "User"
            };

            var result = await controller.Signup(newUser);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var doc = JsonDocument.Parse(json);
            Assert.Equal("Signup successful", doc.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Signup_Should_Return_BadRequest_When_Email_Exists()
        {
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);

            var existingUser = new User
            {
                Email = "existing@example.com",
                PasswordHash = ComputeSha256Hash("existingpass"),
                FullName = "Existing User",
                Role = "User"
            };

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var newUser = new User
            {
                Email = "existing@example.com",
                PasswordHash = "newpass123",
                FullName = "New User",
                Role = "User"
            };

            var result = await controller.Signup(newUser);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            Assert.Equal("Email already exists", doc.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
        {
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);

            var rawPassword = "mypassword";
            var hashedPassword = ComputeSha256Hash(rawPassword);

            var user = new User
            {
                Email = "login@test.com",
                PasswordHash = hashedPassword,
                FullName = "Login User",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Email = "login@test.com",
                PasswordHash = rawPassword
            };

            var result = await controller.Login(loginRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.NotNull(root.GetProperty("token").GetString());
            Assert.Equal("User", root.GetProperty("role").GetString());
            Assert.Equal("Login User", root.GetProperty("fullName").GetString());
        }

        [Fact]
        public async Task Login_Should_Return_Unauthorized_When_Credentials_Are_Invalid()
        {
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);

            var user = new User
            {
                Email = "wronguser@test.com",
                PasswordHash = ComputeSha256Hash("correctpassword"),
                FullName = "Wrong User",
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Email = "wronguser@test.com",
                PasswordHash = "wrongpassword"
            };

            var result = await controller.Login(loginRequest);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var json = JsonSerializer.Serialize(unauthorized.Value);
            var doc = JsonDocument.Parse(json);
            Assert.Equal("Invalid email or password", doc.RootElement.GetProperty("message").GetString());
        }
    }
}
