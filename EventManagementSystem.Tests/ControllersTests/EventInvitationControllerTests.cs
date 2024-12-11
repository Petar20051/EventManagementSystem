using EventMaganementSystem.Controllers;
using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class EventInvitationControllerTests
    {
        private readonly Mock<IEventInvitationService> _mockInvitationService;
        private readonly EventDbContext _dbContext;

        public EventInvitationControllerTests()
        {
            _mockInvitationService = new Mock<IEventInvitationService>();

            
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new EventDbContext(options);

            
            _dbContext.Events.Add(new Event
            {
                Id = 1,
                Name = "Event 1",
                Description = "A sample event description.",
                OrganizerId = "organizer1",
                Date = DateTime.UtcNow
            });

            _dbContext.Users.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "User 1"
            });

            _dbContext.SaveChanges();
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithInvitations()
        {
            
            var userId = "user1";
            var invitations = new List<EventInvitation>
        {
            new EventInvitation { Id = 1, EventId = 1, SenderId = userId, ReceiverId = "user2" }
        };

            _mockInvitationService.Setup(s => s.GetAllInvitationsForUserAsync(userId)).ReturnsAsync(invitations);

            var controller = new EventInvitationController(_mockInvitationService.Object, _dbContext)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EventInvitation>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(userId, viewResult.ViewData["UserId"]);
        }

        [Fact]
        public void Create_GET_ReturnsView_WithEventAndUserLists()
        {
            
            var controller = new EventInvitationController(_mockInvitationService.Object, _dbContext);

            
            var result = controller.Create();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["EventList"]);
            Assert.NotNull(viewResult.ViewData["UserList"]);
        }

        [Fact]
public async Task Create_POST_RedirectsToIndex_WhenModelStateIsValid()
{
    
    var options = new DbContextOptionsBuilder<EventDbContext>()
        .UseInMemoryDatabase(databaseName: "EventInvitationTestDb")
        .Options;

    await using var dbContext = new EventDbContext(options);
    
    
    dbContext.Events.Add(new Event { Id = 1, Name = "Test Event", OrganizerId = "user1", Date = DateTime.UtcNow , Description = "good" });
    dbContext.Users.Add(new ApplicationUser { Id = "user2", UserName = "ReceiverUser" });
    await dbContext.SaveChangesAsync();

    var userId = "user1";
    var invitation = new EventInvitation { EventId = 1, ReceiverId = "user2" };

    var mockInvitationService = new Mock<IEventInvitationService>();
    mockInvitationService.Setup(s => s.SendInvitationAsync(userId, invitation.ReceiverId, invitation.EventId))
                         .Returns(Task.CompletedTask);

    var controller = new EventInvitationController(mockInvitationService.Object, dbContext)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateUserPrincipal(userId) 
            }
        }
    };

    
    var result = await controller.Create(invitation);

    
    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirectResult.ActionName);

    mockInvitationService.Verify(s => s.SendInvitationAsync(userId, invitation.ReceiverId, invitation.EventId), Times.Once);
}



        [Fact]
        public async Task Details_ReturnsViewResult_WithInvitation()
        {
            
            var invitation = new EventInvitation { Id = 1, EventId = 1, ReceiverId = "user2" };
            _mockInvitationService.Setup(s => s.GetInvitationByIdAsync(1)).ReturnsAsync(invitation);

            var controller = new EventInvitationController(_mockInvitationService.Object, _dbContext);

            
            var result = await controller.Details(1);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<EventInvitation>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex_WhenInvitationIsDeleted()
        {
            
            _mockInvitationService.Setup(s => s.DeleteInvitationAsync(1)).Returns(Task.CompletedTask);

            var controller = new EventInvitationController(_mockInvitationService.Object, _dbContext);

            
            var result = await controller.DeleteConfirmed(1);

            
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            _mockInvitationService.Verify(s => s.DeleteInvitationAsync(1), Times.Once);
        }

        [Fact]
        public async Task ConfirmInvitation_RedirectsToIndex_WhenInvitationIsConfirmed()
        {
            
            var invitation = new EventInvitation { Id = 1, EventId = 1 };
            _mockInvitationService.Setup(s => s.GetInvitationByIdAsync(1)).ReturnsAsync(invitation);
            _mockInvitationService.Setup(s => s.ConfirmInvitationAsync(1)).Returns(Task.CompletedTask);

            var controller = new EventInvitationController(_mockInvitationService.Object, _dbContext);

            
            var result = await controller.ConfirmInvitation(1);

            
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            _mockInvitationService.Verify(s => s.ConfirmInvitationAsync(1), Times.Once);
        }
    }
}
