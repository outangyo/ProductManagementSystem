using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ProductManagementSystem.API.Services;
using ProductManagementSystem.Db.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ProductManagementSystem.Tests;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly TokenService _tokenService;
    private const string JwtSecret = "super_secret_key_that_must_be_very_long_at_least_32_bytes_long";

    public TokenServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        
        // Setup configuration mock responses
        _configMock.Setup(c => c["Jwt:Key"]).Returns(JwtSecret);
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("http://localhost:7133");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("http://localhost:4200");
        _configMock.Setup(c => c["Jwt:ExpiryInMinutes"]).Returns("60");

        _tokenService = new TokenService(_configMock.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwt_WhenConfiguredCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testadmin",
            FullName = "Test Admin",
            Role = "Admin"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("http://localhost:7133", jwtToken.Issuer);
        Assert.Contains(jwtToken.Audiences, aud => aud == "http://localhost:4200");
        Assert.Equal("testadmin", jwtToken.Claims.First(c => c.Type == "unique_name").Value);
        Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == "role").Value);
        Assert.Equal("Test Admin", jwtToken.Claims.First(c => c.Type == "FullName").Value);
        Assert.Equal("1", jwtToken.Claims.First(c => c.Type == "nameid").Value);
    }

    [Fact]
    public void GenerateToken_ShouldThrowException_WhenJwtKeyIsMissing()
    {
        // Arrange
        _configMock.Setup(c => c["Jwt:Key"]).Returns((string?)null);
        var user = new User { Id = 1, Username = "user", Role = "User" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _tokenService.GenerateToken(user));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnSecureRandomString()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotEmpty(token1);
        Assert.NotEqual(token1, token2); // Random tokens must not match
        
        // 64 bytes is 88 chars in Base64
        Assert.Equal(88, token1.Length);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldRetrieveClaims_WhenTokenHasCorrectSignature()
    {
        // Arrange
        var user = new User { Id = 5, Username = "staff", Role = "User", FullName = "Staff User" };
        var token = _tokenService.GenerateToken(user);

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal("staff", principal.Identity?.Name);
        Assert.Equal("User", principal.FindFirst(ClaimTypes.Role)?.Value);
        Assert.Equal("5", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldThrowException_WhenTokenSignatureIsInvalid()
    {
        // Arrange
        var user = new User { Id = 5, Username = "staff", Role = "User" };
        var token = _tokenService.GenerateToken(user);
        
        // Corrupt the token signature slightly
        var corruptedToken = token.Substring(0, token.Length - 5) + "aaaaa";

        // Act & Assert
        Assert.ThrowsAny<SecurityTokenException>(() => _tokenService.GetPrincipalFromExpiredToken(corruptedToken));
    }
}
