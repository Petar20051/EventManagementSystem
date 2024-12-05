using EventMaganementSystem.Data;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class EventInvitationServiceTests
    {
        private readonly DbContextOptions<EventDbContext> _options;
        private readonly EventDbContext _context;
        private readonly EventInvitationService _service;

        public EventInvitationServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventDbContext(_options);
            _service = new EventInvitationService(_context);
        }

        private async Task SeedDataAsync()
        {
            _context.Users.AddRange(
                new ApplicationUser { Id = "user1", UserName = "SenderUser" },
                new ApplicationUser { Id = "user2", UserName = "ReceiverUser" },
                new ApplicationUser { Id = "user3", UserName = "AnotherSender" }
            );

            _context.Events.AddRange(
                new Event { Id = 1, Name = "Test Event 1", OrganizerId = "user1", Date = DateTime.UtcNow, Description = "Event 1 Description" },
                new Event { Id = 2, Name = "Test Event 2", OrganizerId = "user3", Date = DateTime.UtcNow, Description = "Event 2 Description" }
            );

            _context.EventInvitations.AddRange(
                new EventInvitation
                {
                    Id = 1,
                    SenderId = "user1",
                    ReceiverId = "user2",
                    EventId = 1,
                    InvitationDate = DateTime.UtcNow,
                    Status = InvitationStatus.Pending
                },
                new EventInvitation
                {
                    Id = 2,
                    SenderId = "user3",
                    ReceiverId = "user2",
                    EventId = 2,
                    InvitationDate = DateTime.UtcNow,
                    Status = InvitationStatus.Accepted
                }
            );

            await _context.SaveChangesAsync();
        }



        [Fact]
        public async Task SendInvitationAsync_AddsNewInvitation()
        {
            // Act
            await _service.SendInvitationAsync("user1", "user3", 1);

            // Assert
            var invitation = await _context.EventInvitations.FirstOrDefaultAsync(i =>
                i.SenderId == "user1" && i.ReceiverId == "user3" && i.EventId == 1);

            Assert.NotNull(invitation);
            Assert.Equal(InvitationStatus.Pending, invitation.Status);
        }

        [Fact]
        public async Task GetPendingInvitationsAsync_ReturnsOnlyPendingInvitations()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            var pendingInvitations = await _service.GetPendingInvitationsAsync("user2");

            // Assert
            Assert.Single(pendingInvitations);
            Assert.Equal(InvitationStatus.Pending, pendingInvitations.First().Status);
        }

        [Fact]
        public async Task GetInvitationByIdAsync_ReturnsCorrectInvitation()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            var invitation = await _service.GetInvitationByIdAsync(1);

            // Assert
            Assert.NotNull(invitation);
            Assert.Equal(1, invitation.Id);
        }


        [Fact]
        public async Task GetInvitationByIdAsync_ThrowsExceptionIfNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetInvitationByIdAsync(999));
        }

        [Fact]
        public async Task DeleteInvitationAsync_RemovesInvitation()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            await _service.DeleteInvitationAsync(1);

            // Assert
            var invitation = await _context.EventInvitations.FindAsync(1);
            Assert.Null(invitation);
        }

        [Fact]
        public async Task RespondToInvitationAsync_UpdatesStatusToAccepted()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            await _service.RespondToInvitationAsync(1, true);

            // Assert
            var invitation = await _context.EventInvitations.FindAsync(1);
            Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        }

        [Fact]
        public async Task RespondToInvitationAsync_UpdatesStatusToDeclined()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            await _service.RespondToInvitationAsync(1, false);

            // Assert
            var invitation = await _context.EventInvitations.FindAsync(1);
            Assert.Equal(InvitationStatus.Declined, invitation.Status);
        }

        [Fact]
        public async Task GetInvitationsForEventAsync_ReturnsInvitationsForSpecificEvent()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            var invitations = await _service.GetInvitationsForEventAsync(1);

            // Assert
            Assert.Single(invitations);
            Assert.Equal(1, invitations.First().EventId);
        }

        [Fact]
        public async Task GetAllInvitationsForUserAsync_ReturnsAllRelatedInvitations()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            var invitations = await _service.GetAllInvitationsForUserAsync("user2");

            // Assert
            Assert.Equal(2, invitations.Count());
        }


        [Fact]
        public async Task ConfirmInvitationAsync_UpdatesStatusToAccepted()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            await _service.ConfirmInvitationAsync(1);

            // Assert
            var invitation = await _context.EventInvitations.FindAsync(1);
            Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        }

        [Fact]
        public async Task ConfirmInvitationAsync_ThrowsExceptionIfNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ConfirmInvitationAsync(999));
        }
    }
}
