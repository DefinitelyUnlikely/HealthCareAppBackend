// using Moq;
// using HealthCareAB_v1.Services;
// using HealthCareAB_v1.Repositories.Interfaces;
// using HealthCareAB_v1.Models;
// using Microsoft.EntityFrameworkCore;
// using HealthCareAB_v1.Repositories;

// namespace HealthCareApp.Tests;

// public class MeetingRepositoryTests
// {

//     [Fact]
//     public async Task MeetingTimeDoesNotOverlap_ReturnsFalse()
//     {
//         // Arrange
//         var context = TestContext();
//         var validMeetingTime = new Meeting
//         {
//             CaregiverId = 1,
//             StartTime = new DateTime(2024, 1, 1, 9, 30, 0),
//             EndTime = new DateTime(2024, 1, 1, 10, 0, 0)
//         };
//         var repo = new MeetingRepository(context);

//         // Act
//         var exists = await repo.TimeUnavailableAsync(validMeetingTime);

//         // Assert
//         Assert.False(exists);
//     }

//     [Fact]
//     public async Task MeetingTimeOverlaps_ReturnsTrue()
//     {
//         // Arrange
//         var context = TestContext();
//         var invalidMeetingTime = new Meeting
//         {
//             CaregiverId = 1,
//             StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
//             EndTime = new DateTime(2024, 1, 1, 10, 30, 0)
//         };
//         var repo = new MeetingRepository(context);

//         // Act
//         var exists = await repo.TimeUnavailableAsync(invalidMeetingTime);

//         // Assert
//         Assert.True(exists);
//     }

//     [Fact]
//     public async Task GetMeetingsByUserId_ReturnsUpcomingMeetingsByUserId()
//     {
//         // Arrange
//         var context = TestContext();
//         var repo = new MeetingRepository(context);

//         // Act
//         var meetings = await repo.GetByUserIdAsync(3, false);

//         // Assert
//         Assert.Equal(2, meetings.Count());
//     }

//     [Fact]
//     public async Task GetMeetingsByUserId_HistoricTrue_ReturnsAllMeetingsByUserId()
//     {
//         // Arrange
//         var context = TestContext();
//         var repo = new MeetingRepository(context);

//         // Act
//         var meetings = await repo.GetByUserIdAsync(3, true);

//         // Assert
//         Assert.Equal(4, meetings.Count());
//     }

//     [Fact]
//     public async Task GetMeetingsByUserId_UserIsCaregiver_ReturnsAllMeetingsByUserId()
//     {
//         // Arrange
//         var context = TestContext();
//         var repo = new MeetingRepository(context);

//         // Act
//         var meetings = await repo.GetByUserIdAsync(1, true);

//         // Assert
//         Assert.Equal(5, meetings.Count());
//     }

//     private AppDbContext TestContext()
//     {
//         var options = new DbContextOptionsBuilder<AppDbContext>()
//         .UseInMemoryDatabase(Guid.NewGuid().ToString())
//         .Options;

//         var context = new AppDbContext(options);
//         var today = DateTime.Now.Date;
//         context.Users.Add(new Patient { Id = 3, Username = "TestPatient" });
//         context.Users.Add(new Patient { Id = 2, Username = "TestPatient2" });
//         context.Users.Add(new Caregiver { Id = 1, Username = "TestCaregiver" });
//         var existingMeetings = new List<Meeting>
//         {
//             new Meeting
//             {
//                 PatientId = 2,
//                 CaregiverId = 1,
//                 StartTime = new DateTime(2024, 1, 1, 9, 0, 0),
//                 EndTime   = new DateTime(2024, 1, 1, 9, 30, 0)
//             },
//             new Meeting
//             {
//                 PatientId = 3,
//                 CaregiverId = 1,
//                 StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
//                 EndTime   = new DateTime(2024, 1, 1, 11, 0, 0)
//             },
//             new Meeting
//             {
//                 PatientId = 3,
//                 CaregiverId = 1,
//                 StartTime = new DateTime(2024, 1, 1, 11, 0, 0),
//                 EndTime   = new DateTime(2024, 1, 1, 12, 0, 0)
//             },
//             new Meeting
//             {
//                 PatientId = 3,
//                 CaregiverId = 1,
//                 StartTime = today.AddDays(1).AddHours(8),
//                 EndTime   = today.AddDays(1).AddHours(8).AddMinutes(30),
//             },
//             new Meeting
//             {
//                 PatientId = 3,
//                 CaregiverId = 1,
//                 StartTime = today.AddDays(2).AddHours(8),
//                 EndTime   = today.AddDays(2).AddHours(8).AddMinutes(30),
//             }
//         };

//         context.Meetings.AddRange(existingMeetings);
//         context.SaveChanges();

//         return context;
//     }
// }
