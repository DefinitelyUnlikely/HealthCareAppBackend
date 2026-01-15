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

public class LoginTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkAndSetsCookie()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var loginDto = new LoginDto { Username = "test", Password = "password" };
        var authResponse = new AuthResponseDto
        { Success = true, Message = "Login successful", Username = "test", Roles = new List<string> { "User" } };
        var token = "valid_token";

        authServiceMock.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync((authResponse, token));

        authServiceMock.Setup(s => s.GetJwtCookieOptions())
            .Returns(new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

        var httpContext = new DefaultHttpContext();
        var controller = new AuthController(authServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var setCookieHeader = httpContext.Response.Headers.SetCookie;
        Assert.Single(setCookieHeader);
        Assert.Contains($"{CookieNames.Jwt}={token}", setCookieHeader.ToString());
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var loginDto = new LoginDto { Username = "test", Password = "wrong_password" };
        var authResponse = new AuthResponseDto { Success = false, Message = "Invalid username or password" };

        authServiceMock.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync((authResponse, null));

        var controller = new AuthController(authServiceMock.Object);

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_EmptyToken_ReturnsUnauthorized_WhenSuccessIsFalse()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var loginDto = new LoginDto { Username = "test", Password = "password" };
        var authResponse = new AuthResponseDto { Success = true, Message = "Login successful?" };
        var token = "";

        authServiceMock.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync((authResponse, token));

        var controller = new AuthController(authServiceMock.Object);

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ValidUserAndPassword_ReturnsSuccessAndToken()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        var environmentMock = new Mock<IWebHostEnvironment>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());

        var user = new User
        {
            Username = "validuser",
            PasswordHash = "hashedPassword",
            Roles = new List<string> { "User" }
        };

        userServiceMock.Setup(u => u.GetUserByUsernameAsync("validuser"))
            .ReturnsAsync(user);

        userServiceMock.Setup(u => u.VerifyPassword("password123", "hashedPassword"))
            .Returns(true);

        jwtTokenServiceMock.Setup(t => t.GenerateToken(user))
            .Returns("valid_token");

        var authService = new AuthService(
            userServiceMock.Object,
            jwtTokenServiceMock.Object,
            jwtSettingsMock.Object,
            environmentMock.Object,
            httpContextAccessorMock.Object);

        var loginDto = new LoginDto
        {
            Username = "validuser",
            Password = "password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.True(result.response.Success);
        Assert.Equal("Login successful", result.response.Message);
        Assert.Equal("validuser", result.response.Username);
        Assert.Equal("valid_token", result.token);

        userServiceMock.Verify(u => u.GetUserByUsernameAsync("validuser"), Times.Once);
        userServiceMock.Verify(u => u.VerifyPassword("password123", "hashedPassword"), Times.Once);
        jwtTokenServiceMock.Verify(t => t.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        var environmentMock = new Mock<IWebHostEnvironment>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());

        userServiceMock.Setup(u => u.GetUserByUsernameAsync("nonexistent"))
            .ReturnsAsync((User?)null);

        var authService = new AuthService(
            userServiceMock.Object,
            jwtTokenServiceMock.Object,
            jwtSettingsMock.Object,
            environmentMock.Object,
            httpContextAccessorMock.Object);

        var loginDto = new LoginDto
        {
            Username = "nonexistent",
            Password = "password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.response.Success);
        Assert.Equal("Invalid username or password", result.response.Message);
        Assert.Null(result.token);

        userServiceMock.Verify(u => u.GetUserByUsernameAsync("nonexistent"), Times.Once);
        userServiceMock.Verify(u => u.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        var environmentMock = new Mock<IWebHostEnvironment>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings());

        var user = new User
        {
            Username = "validuser",
            PasswordHash = "hashedPassword"
        };

        userServiceMock.Setup(u => u.GetUserByUsernameAsync("validuser"))
            .ReturnsAsync(user);

        userServiceMock.Setup(u => u.VerifyPassword("wrongpassword", "hashedPassword"))
            .Returns(false);

        var authService = new AuthService(
            userServiceMock.Object,
            jwtTokenServiceMock.Object,
            jwtSettingsMock.Object,
            environmentMock.Object,
            httpContextAccessorMock.Object);

        var loginDto = new LoginDto
        {
            Username = "validuser",
            Password = "wrongpassword"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.response.Success);
        Assert.Equal("Invalid username or password", result.response.Message);
        Assert.Null(result.token);

        userServiceMock.Verify(u => u.GetUserByUsernameAsync("validuser"), Times.Once);
        userServiceMock.Verify(u => u.VerifyPassword("wrongpassword", "hashedPassword"), Times.Once);
    }
}

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

// SEE BELOW FOR Antigravity's Agent Prompt for login tests
// Hey! I want to create a few tests for the login endpoint
// and the corresponding authentication service method LoginAsync.
// I want the controller endpoint to test
// 1. That the authservice is called and when valid results are returned,
// we set the cookie and return the Ok result.
// 2. That the authservice is called and if the result is not a succes,
// we return unauthorized
// 3. The same as 2, but we check if we get the unauthorized response when the token is an empty string.

// For the authService, I want to
// 1. Check that the userService is called with both GetUser and VerifyPassword
// and then that the jwttoken service is called.
// And that we do a return with a successful result.
// 2. Same as above,
// but we return the result for user == null
// 3. same as above but we pretend the password doesn't pass and we send
// send back the result with success = false (i.e. same as #2)