using Moq;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;

namespace HealthCareApp.Tests;

public class MeetingControllerTests
{
    [Fact]
    public async Task GetMeeting_NoValidClaim_ReturnsUnauthorized()
    {
        // Arrange
        var mockService = new Mock<IMeetingService>();
        var controller = new MeetingController(mockService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
            }
        };

        // Act
        var result = await controller.GetMeeting(Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetMeeting_ServiceFails_ReturnsNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var mockService = new Mock<IMeetingService>();
        mockService
            .Setup(s => s.GetMeetingAsync(meetingId, 1, false))
            .ReturnsAsync(new MeetingResponseDto { Success = false, Message = "Meeting not found" });

        var controller = new MeetingController(mockService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "1")
                ]))
            }
        };

        // Act
        var result = await controller.GetMeeting(meetingId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetMeeting_ServiceSucceeds_ReturnsOk()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new MeetingDto { Id = meetingId, };

        var mockService = new Mock<IMeetingService>();
        mockService
            .Setup(s => s.GetMeetingAsync(meetingId, 1, false))
            .ReturnsAsync(new MeetingResponseDto { Success = true, Meeting = meeting });

        var controller = new MeetingController(mockService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "1")
                ]))
            }
        };

        // Act
        var result = await controller.GetMeeting(meetingId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<MeetingResponseDto>(ok.Value);
        Assert.Equal(meeting, returned.Meeting);
    }
}
