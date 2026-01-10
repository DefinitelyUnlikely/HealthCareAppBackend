using System.Security.Claims;
using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.Controllers;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace HealthCareApp.Tests;

public class LogoutTests
{
    [Fact]
    public void LogoutEndpoint_CallsAuthServiceGetClearCookieOptions()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.GetClearCookieOptions()).Returns(new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
        var controller = new AuthController(authServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
                }
            }
        };


        // Act
        controller.Logout();

        // Assert
        authServiceMock.Verify(s => s.GetClearCookieOptions(), Times.Once());
    }

    [Fact]
    public void LogoutEndpoint_ReturnsOkObjectResult_WhenCalled()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.GetClearCookieOptions()).Returns(new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
        var controller = new AuthController(authServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
                }
            }
        };

        // Act
        var result = controller.Logout();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void LogoutEndpoint_RemovesCookie_WhenCalled()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(s => s.GetClearCookieOptions()).Returns(new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });

        // Asked the antigravity agent for help with how to set and find cookies in a unit test. 
        var httpContext = new DefaultHttpContext();
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        requestCookiesMock.Setup(c => c.ContainsKey(CookieNames.Jwt)).Returns(true);
        requestCookiesMock.Setup(c => c[CookieNames.Jwt]).Returns("test-token");
        httpContext.Request.Cookies = requestCookiesMock.Object;

        var controller = new AuthController(authServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        // Act
        controller.Logout();

        // Assert
        var setCookieHeader = httpContext.Response.Headers.SetCookie;
        Assert.True(setCookieHeader.Count > 0);
        var cookieString = setCookieHeader.ToString();
        Assert.Contains($"{CookieNames.Jwt}=", cookieString);
        Assert.Contains("expires=", cookieString);
    }
}

public class AuthServiceTests
{
    [Fact]
    public void GetClearCookieOptions_ReturnsCorrectOptions()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        var environmentMock = new Mock<IWebHostEnvironment>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());
        environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        var authService = new AuthService(
            userServiceMock.Object,
            jwtTokenServiceMock.Object,
            jwtSettingsMock.Object,
            environmentMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var options = authService.GetClearCookieOptions();

        // Assert
        Assert.True(options.HttpOnly);
        Assert.True(options.Secure);
        Assert.Equal(SameSiteMode.Strict, options.SameSite);
        Assert.Equal("/", options.Path);
        Assert.NotNull(options.Expires);
        // Just assert that expires is in the past, exact time is not important and harder to test.
        Assert.True(options.Expires < DateTimeOffset.UtcNow);
    }



    [Fact]
public async Task RegisterAsync_ReturnsSuccessFalse_WhenUsernameAlreadyExists()
{
    // Arrange
    var userServiceMock = new Mock<IUserService>();
    var jwtTokenServiceMock = new Mock<IJwtTokenService>();
    var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
    var environmentMock = new Mock<IWebHostEnvironment>();
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

    jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());
    environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
    
    userServiceMock.Setup(s => s.ExistsByUsernameAsync("existinguser"))
        .ReturnsAsync(true);

    var authService = new AuthService(
        userServiceMock.Object,
        jwtTokenServiceMock.Object,
        jwtSettingsMock.Object,
        environmentMock.Object,
        httpContextAccessorMock.Object);

    var registerDto = new RegisterDto
    {
        Username = "existinguser",
        Password = "password123",
        Email = "test@example.com",
        FirstName = "John",
        LastName = "Doe",
        PhoneNumber = "1234567890",
        Address = "123 Main Street",
        PersonalNumber = "1234567890"
    };

    // Act
    var result = await authService.RegisterAsync(registerDto);

    // Assert
    Assert.False(result.Success);
    Assert.Equal("Username is already taken", result.Message);
    userServiceMock.Verify(s => s.CreateUserAsync(It.IsAny<User>()), Times.Never());
}

[Fact]
public async Task RegisterAsync_CreatesUserSuccessfully_WithDefaultUserRole()
{
    // Arrange
    var userServiceMock = new Mock<IUserService>();
    var jwtTokenServiceMock = new Mock<IJwtTokenService>();
    var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
    var environmentMock = new Mock<IWebHostEnvironment>();
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

    jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());
    environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
    
    userServiceMock.Setup(s => s.ExistsByUsernameAsync(It.IsAny<string>()))
        .ReturnsAsync(false);
    userServiceMock.Setup(s => s.HashPassword(It.IsAny<string>()))
        .Returns("hashedPassword");

    var authService = new AuthService(
        userServiceMock.Object,
        jwtTokenServiceMock.Object,
        jwtSettingsMock.Object,
        environmentMock.Object,
        httpContextAccessorMock.Object);

    var registerDto = new RegisterDto
    {
        Username = "newuser",
        Password = "password123",
        Email = "test@example.com",
        FirstName = "John",
        LastName = "Doe",
        PhoneNumber = "1234567890",
        Address = "123 Main Street",
        PersonalNumber = "1234567890"
    };

    // Act
    var result = await authService.RegisterAsync(registerDto);

    // Assert
    Assert.True(result.Success);
    Assert.Equal("User registered successfully", result.Message);
    Assert.Equal("newuser", result.Username);
    Assert.NotNull(result.Roles);
    Assert.Single(result.Roles);
    Assert.Contains(Roles.User, result.Roles);
    
    userServiceMock.Verify(s => s.CreateUserAsync(It.Is<User>(u => 
        u.Username == "newuser" && 
        u.PasswordHash == "hashedPassword" &&
        u.Roles.Count == 1 &&
        u.Roles.Contains(Roles.User))), Times.Once());
}

[Fact]
public async Task RegisterAsync_CreatesUserSuccessfully_WithRequestedRoles()
{
    // Arrange
    var userServiceMock = new Mock<IUserService>();
    var jwtTokenServiceMock = new Mock<IJwtTokenService>();
    var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
    var environmentMock = new Mock<IWebHostEnvironment>();
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

    jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());
    environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
    
    userServiceMock.Setup(s => s.ExistsByUsernameAsync(It.IsAny<string>()))
        .ReturnsAsync(false);
    userServiceMock.Setup(s => s.HashPassword(It.IsAny<string>()))
        .Returns("hashedPassword");

    var authService = new AuthService(
        userServiceMock.Object,
        jwtTokenServiceMock.Object,
        jwtSettingsMock.Object,
        environmentMock.Object,
        httpContextAccessorMock.Object);

    var registerDto = new RegisterDto
    {
        Username = "adminuser",
        Password = "password123",
        Email = "admin@example.com",
        FirstName = "Admin",
        LastName = "User",
        PhoneNumber = "1234567890",
        Address = "123 Admin Street",
        PersonalNumber = "1234567890",
        Roles = new List<string> { Roles.Admin }
    };

    // Act
    var result = await authService.RegisterAsync(registerDto);

    // Assert
    Assert.True(result.Success);
    Assert.Equal("User registered successfully", result.Message);
    Assert.Equal("adminuser", result.Username);
    Assert.NotNull(result.Roles);
    Assert.Single(result.Roles);
    Assert.Contains(Roles.Admin, result.Roles);
    
    userServiceMock.Verify(s => s.CreateUserAsync(It.Is<User>(u => 
        u.Username == "adminuser" && 
        u.Roles.Contains(Roles.Admin))), Times.Once());
}

[Fact]
public async Task RegisterAsync_ThrowsArgumentNullException_WhenRegisterDtoIsNull()
{
    // Arrange
    var userServiceMock = new Mock<IUserService>();
    var jwtTokenServiceMock = new Mock<IJwtTokenService>();
    var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
    var environmentMock = new Mock<IWebHostEnvironment>();
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

    jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());
    
    var authService = new AuthService(
        userServiceMock.Object,
        jwtTokenServiceMock.Object,
        jwtSettingsMock.Object,
        environmentMock.Object,
        httpContextAccessorMock.Object);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(() => 
        authService.RegisterAsync(null!));
}

[Fact]
public async Task RegisterAsync_CallsHashPassword_WithCorrectPassword()
{
    // Arrange
    var userServiceMock = new Mock<IUserService>();
    var jwtTokenServiceMock = new Mock<IJwtTokenService>();
    var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
    var environmentMock = new Mock<IWebHostEnvironment>();
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

    jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());
    environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
    
    userServiceMock.Setup(s => s.ExistsByUsernameAsync(It.IsAny<string>()))
        .ReturnsAsync(false);
    userServiceMock.Setup(s => s.HashPassword("mySecurePassword123"))
        .Returns("hashedPassword");

    var authService = new AuthService(
        userServiceMock.Object,
        jwtTokenServiceMock.Object,
        jwtSettingsMock.Object,
        environmentMock.Object,
        httpContextAccessorMock.Object);

    var registerDto = new RegisterDto
    {
        Username = "testuser",
        Password = "mySecurePassword123",
        Email = "test@example.com",
        FirstName = "Test",
        LastName = "User",
        PhoneNumber = "1234567890",
        Address = "123 Test Street",
        PersonalNumber = "1234567890"
    };

    // Act
    await authService.RegisterAsync(registerDto);

    // Assert
    userServiceMock.Verify(s => s.HashPassword("mySecurePassword123"), Times.Once());
}
}
