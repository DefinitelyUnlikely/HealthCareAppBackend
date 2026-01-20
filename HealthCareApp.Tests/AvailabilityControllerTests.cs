using Moq;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using HealthCareAB_v1.DTOs.Availability;
using HealthCareAB_v1.Models;

namespace HealthCareApp.Tests;

public class AvailabilityControllerTests
{
    private readonly Mock<IAvailabilityService> _mockService;
    private readonly AvailabilityController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public AvailabilityControllerTests()
    {
        _mockService = new Mock<IAvailabilityService>();
        _controller = new AvailabilityController(_mockService.Object);

        // Default authenticated user setup
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _userPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = _userPrincipal
            }
        };
    }

    [Fact]
    public async Task GetAvailability_NotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated
        var request = new GetAvailabilityRequest { From = DateTime.Now, To = DateTime.Now.AddDays(1) };

        // Act
        var result = await _controller.GetAvailability(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetAvailability_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new GetAvailabilityRequest { From = DateTime.Now, To = DateTime.Now.AddDays(1) };
        var availability = new List<Availability>();
        _mockService.Setup(s => s.GetAvailabilityAsync(1, request.From, request.To))
            .ReturnsAsync(availability);

        // Act
        var result = await _controller.GetAvailability(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(availability, okResult.Value);
    }

    [Fact]
    public async Task GetAvailability_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = new GetAvailabilityRequest { From = DateTime.Now, To = DateTime.Now.AddDays(1) };
        _mockService.Setup(s => s.GetAvailabilityAsync(1, request.From, request.To))
            .ThrowsAsync(new ArgumentException("Invalid range"));

        // Act
        var result = await _controller.GetAvailability(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid range", badRequestResult.Value!.ToString());
    }

    [Fact]
    public async Task GetAvailability_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new GetAvailabilityRequest { From = DateTime.Now, To = DateTime.Now.AddDays(1) };
        _mockService.Setup(s => s.GetAvailabilityAsync(1, request.From, request.To))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.GetAvailability(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public async Task SetAvailability_NotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated
        var request = new SetAvailabilityRequest { UserId = 1, From = DateTime.Now, To = DateTime.Now.AddDays(1) };

        // Act
        var result = await _controller.SetAvailability(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task SetAvailability_DifferentUserNotAdmin_ReturnsUnauthorized()
    {
        // Arrange
        var request = new SetAvailabilityRequest { UserId = 2, From = DateTime.Now, To = DateTime.Now.AddDays(1) };

        // Act
        var result = await _controller.SetAvailability(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task SetAvailability_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new SetAvailabilityRequest { UserId = 1, From = DateTime.Now, To = DateTime.Now.AddDays(1) };
        _mockService.Setup(s => s.SetAvailableAsync(1, request.From, request.To))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SetAvailability(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        // Verify service call
        _mockService.Verify(s => s.SetAvailableAsync(1, request.From, request.To), Times.Once);
    }

    [Fact]
    public async Task SetUnavailable_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new SetAvailabilityRequest { UserId = 1, From = DateTime.Now, To = DateTime.Now.AddDays(1), ForceCancel = true };
        _mockService.Setup(s => s.SetUnavailableAsync(1, request.From, request.To, true))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SetUnavailable(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(s => s.SetUnavailableAsync(1, request.From, request.To, true), Times.Once);
    }
}
