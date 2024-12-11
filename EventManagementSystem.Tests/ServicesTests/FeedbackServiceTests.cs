using EventMaganementSystem.Data;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class FeedbackServiceTests
    {
        private readonly DbContextOptions<EventDbContext> _options;

        public FeedbackServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private FeedbackService CreateService(EventDbContext dbContext)
        {
            return new FeedbackService(dbContext);
        }

        private async Task SeedDataAsync(EventDbContext dbContext)
        {
            
            dbContext.Users.AddRange(
                new ApplicationUser { Id = "user1", UserName = "Test User 1" },
                new ApplicationUser { Id = "user2", UserName = "Test User 2" }
            );

            
            dbContext.Events.AddRange(
                new Event
                {
                    Id = 1,
                    Name = "Event 1",
                    Date = DateTime.UtcNow.AddDays(5),
                    Description = "Event 1 description",
                    OrganizerId = "user1"
                },
                new Event
                {
                    Id = 2,
                    Name = "Event 2",
                    Date = DateTime.UtcNow.AddDays(10),
                    Description = "Event 2 description",
                    OrganizerId = "user2"
                }
            );

            
            dbContext.Feedbacks.AddRange(
                new Feedback
                {
                    Id = 1,
                    EventId = 1,
                    UserId = "user1",
                    FeedbackContent = "Great event!",
                    FeedbackDate = DateTime.UtcNow.AddDays(-1)
                },
                new Feedback
                {
                    Id = 2,
                    EventId = 1,
                    UserId = "user2",
                    FeedbackContent = "It was okay.",
                    FeedbackDate = DateTime.UtcNow.AddDays(-2)
                }
            );

            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task AddFeedbackAsync_AddsFeedbackSuccessfully()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var newFeedback = new Feedback
            {
                EventId = 2,
                UserId = "user1",
                FeedbackContent = "Amazing event!",
                FeedbackDate = DateTime.UtcNow
            };

            
            await service.AddFeedbackAsync(newFeedback);

            
            var addedFeedback = await dbContext.Feedbacks.FirstOrDefaultAsync(f => f.FeedbackContent == "Amazing event!");
            Assert.NotNull(addedFeedback);
            Assert.Equal(2, addedFeedback.EventId);
            Assert.Equal("user1", addedFeedback.UserId);
        }

        [Fact]
        public async Task GetFeedbacksByEventIdAsync_ReturnsFeedbacksForEvent()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            
            var feedbacks = await service.GetFeedbacksByEventIdAsync(1);

            
            Assert.Equal(2, feedbacks.Count());
            Assert.Equal("Great event!", feedbacks.First().FeedbackContent);
            Assert.Equal("It was okay.", feedbacks.Last().FeedbackContent);
        }

        [Fact]
        public async Task GetFeedbacksByEventIdAsync_ReturnsEmptyIfNoFeedbacks()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            
            var feedbacks = await service.GetFeedbacksByEventIdAsync(99); 

            
            Assert.Empty(feedbacks);
        }
    }
}
