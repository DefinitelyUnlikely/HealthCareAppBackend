using System.Security.Claims;
using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.Controllers;
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
}