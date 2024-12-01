using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class TicketControllerTests
    {
        private readonly Mock<ITicketService> _mockTicketService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public TicketControllerTests()
        {
            _mockTicketService = new Mock<ITicketService>();
            _mockUserManager = MockUserManager();
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task MyTickets_ReturnsViewResult_WithTickets()
        {
            // Arrange
            var userId = "user1";
            var tickets = new List<Ticket>
        {
            new Ticket { Id = 1, Event = new Event { Name = "Event 1" }, HolderId = userId },
            new Ticket { Id = 2, Event = new Event { Name = "Event 2" }, HolderId = userId }
        };

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _mockTicketService.Setup(s => s.GetUserTicketsAsync(userId)).ReturnsAsync(tickets);

            var controller = new TicketController(_mockTicketService.Object, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            // Act
            var result = await controller.MyTickets();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Ticket>>(viewResult.Model);
            Assert.Equal(2, model.Count);

            foreach (var ticket in model)
            {
                Assert.NotNull(ticket.QRCodeSvg); // Verify QR code is generated
                Assert.Contains("<svg", ticket.QRCodeSvg); // Ensure the QR code is in SVG format
            }
        }

        [Fact]
        public async Task MyTickets_ReturnsEmptyList_WhenUserHasNoTickets()
        {
            // Arrange
            var userId = "user1";

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _mockTicketService.Setup(s => s.GetUserTicketsAsync(userId)).ReturnsAsync(new List<Ticket>());

            var controller = new TicketController(_mockTicketService.Object, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            // Act
            var result = await controller.MyTickets();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Ticket>>(viewResult.Model);
            Assert.Empty(model);
        }
    }
}
