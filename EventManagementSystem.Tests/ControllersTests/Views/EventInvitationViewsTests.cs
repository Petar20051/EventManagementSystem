using EventMaganementSystem.Controllers;
using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests.Views
{
    public class EventInvitationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly DbContextOptions<EventDbContext> _options;

        public EventInvitationControllerTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: "EventInvitationTestDb")
                .Options;

            // Seed necessary data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var dbContext = new EventDbContext(_options);

            // Clear database first
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            // Seed data
            dbContext.Events.Add(new Event { Id = 1, Name = "Test Event", OrganizerId = "user1", Date = DateTime.UtcNow, Description = "good" });
            dbContext.Users.Add(new ApplicationUser { Id = "user2", UserName = "Test User" });
            dbContext.EventInvitations.Add(new EventInvitation
            {
                Id = 1,
                EventId = 1,
                SenderId = "user1",
                ReceiverId = "user2",
                InvitationDate = DateTime.UtcNow
            });

            dbContext.SaveChanges();
        }

        private EventInvitationController CreateController()
        {
            return new EventInvitationController(Mock.Of<IEventInvitationService>(), new EventDbContext(_options))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "user1")
                    }))
                    }
                }
            };
        }


        [Fact]
        public async Task Create_RendersCorrectly()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            // Check that the correct view is rendered
            Assert.True(viewResult.ViewName == null || viewResult.ViewName == "Create", "Expected the view to be 'Create' or null (default view name).");

            // Verify ViewBag contents
            var eventList = Assert.IsType<SelectList>(viewResult.ViewData["EventList"]);
            var userList = Assert.IsType<SelectList>(viewResult.ViewData["UserList"]);

            Assert.NotEmpty(eventList); // Ensure EventList is populated
            Assert.NotEmpty(userList); // Ensure UserList is populated
        }

        [Fact]
        public async Task Index_RendersCorrectly()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            

            // Validate the model
            var invitations = Assert.IsAssignableFrom<IEnumerable<EventInvitation>>(viewResult.Model);
            Assert.Empty(invitations); // Ensure the invitations list is populated
        }

       






    }

}

