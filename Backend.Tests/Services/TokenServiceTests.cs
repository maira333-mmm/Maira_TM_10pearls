using Xunit;
using Microsoft.Extensions.Configuration;
using Backend.Models;
using Backend.Services;
using System.Collections.Generic;

namespace Backend.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "01234567890123456789012345678901"},  // 32 chars = 256 bits
                {"Jwt:Issuer", "TestIssuer"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _tokenService = new TokenService(configuration);
        }

        [Fact]
        public void GenerateToken_Returns_ValidToken()
        {
            var testUser = new User
            {
                Id = 1,
                Email = "test@example.com",
                Role = "User"
            };

            var token = _tokenService.GenerateToken(testUser);

            Assert.False(string.IsNullOrEmpty(token));
            Assert.Contains("ey", token); // Optional: JWT tokens start with 'ey'
        }
    }
}

