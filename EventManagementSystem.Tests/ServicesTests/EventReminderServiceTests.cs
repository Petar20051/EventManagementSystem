using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class EventReminderServiceTests
    {
        private readonly DbContextOptions<EventDbContext> _options;
        private readonly Mock<INotificationService> _mockNotificationService;

        public EventReminderServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockNotificationService = new Mock<INotificationService>();
        }

        private async Task SeedDataAsync(EventDbContext dbContext)
        {
            dbContext.Users.AddRange(
                new ApplicationUser { Id = "user1", UserName = "OrganizerUser" },
                new ApplicationUser { Id = "user2", UserName = "AttendeeUser" }
            );

            dbContext.Events.AddRange(
                new Event
                {
                    Id = 1,
                    Name = "Upcoming Event",
                    Date = DateTime.UtcNow.Date.AddDays(1),
                    Description = "Test event happening tomorrow.",
                    OrganizerId = "user1" 
                },
                new Event
                {
                    Id = 2,
                    Name = "Past Event",
                    Date = DateTime.UtcNow.Date.AddDays(-1),
                    Description = "Test event that already occurred.",
                    OrganizerId = "user1" 
                }
            );

            dbContext.Reservations.Add(new Reservation
            {
                Id = 1,
                EventId = 1,
                UserId = "user2"
            });

            await dbContext.SaveChangesAsync();
        }


        private EventReminderService CreateService(IServiceProvider serviceProvider)
        {
            return new EventReminderService(_mockNotificationService.Object, serviceProvider);
        }

        [Fact]
        public async Task ExecuteAsync_SendsReminderNotificationsForUpcomingEvents()
        {
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton(new EventDbContext(_options))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
                await SeedDataAsync(dbContext);
            }

            var service = CreateService(serviceProvider);

            
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2)); 

            await service.StartAsync(cancellationTokenSource.Token);

            
            _mockNotificationService.Verify(
                n => n.NotifyEventReminderAsync("user2", "Upcoming Event", It.IsAny<DateTime>()),
                Times.Once);

            _mockNotificationService.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task ExecuteAsync_DoesNotSendRemindersForPastEvents()
        {
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton(new EventDbContext(_options))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
                await SeedDataAsync(dbContext);
            }

            var service = CreateService(serviceProvider);

            
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2)); 

            await service.StartAsync(cancellationTokenSource.Token);

            
            _mockNotificationService.Verify(
                n => n.NotifyEventReminderAsync("user1", "Past Event", It.IsAny<DateTime>()),
                Times.Never);
        }
    }
}
