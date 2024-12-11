using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Core.Models.Sponsorship;
using EventManagementSystem.Infrastructure.Data.Enums;
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
    public class SponsorshipControllerTests
    {
        private readonly Mock<ISponsorshipService> _mockSponsorshipService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IStripePaymentService> _mockStripePaymentService;
        private readonly Mock<IEventService> _mockEventService;

        public SponsorshipControllerTests()
        {
            _mockSponsorshipService = new Mock<ISponsorshipService>();
            _mockUserManager = MockUserManager();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockStripePaymentService = new Mock<IStripePaymentService>();
            _mockEventService = new Mock<IEventService>();
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
        public async Task ProcessSponsorship_POST_ThrowsArgumentNullException_WhenModelIsNull()
        {
            
            var controller = new SponsorshipController(
                _mockSponsorshipService.Object,
                _mockUserManager.Object,
                _mockPaymentService.Object,
                _mockStripePaymentService.Object,
                _mockEventService.Object);

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => controller.ProcessSponsorship(null));
        }

        [Fact]
        public async Task ProcessSponsorship_POST_ReturnsView_WhenNoPaymentMethodSelected()
        {
            
            var model = new ProcessSponsorshipViewModel();
            var controller = new SponsorshipController(
                _mockSponsorshipService.Object,
                _mockUserManager.Object,
                _mockPaymentService.Object,
                _mockStripePaymentService.Object,
                _mockEventService.Object);

            
            var result = await controller.ProcessSponsorship(model);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

       
        [Fact]
        public async Task SponsorshipDashboard_ReturnsView_WithCorrectViewModel()
        {
            
            var userId = "user1";
            var user = new ApplicationUser { Id = userId, SponsoredAmount = 200, SponsorshipTier = SponsorshipTier.Gold };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            var controller = new SponsorshipController(
                _mockSponsorshipService.Object,
                _mockUserManager.Object,
                _mockPaymentService.Object,
                _mockStripePaymentService.Object,
                _mockEventService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            
            var result = await controller.SponsorshipDashboard();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SponsorshipDashboardViewModel>(viewResult.Model);
            Assert.Equal(SponsorshipTier.Gold, model.CurrentTier);
            Assert.Equal(200, model.SponsoredAmount);
        }

        [Fact]
        public async Task SponsorEventList_ReturnsView_WithAvailableEvents()
        {
            
            var events = new List<Event>
    {
        new Event
        {
            Id = 1,
            Name = "Event 1",
            Date = DateTime.Now,
            Venue = new Venue { Name = "Venue 1" },
            Description = "Description 1",
            Organizer = new ApplicationUser { UserName = "Organizer1" }
        },
        new Event
        {
            Id = 2,
            Name = null, 
            Date = DateTime.Now,
            Venue = null, 
            Description = null, 
            Organizer = new ApplicationUser { UserName = null } 
        }
    };

            _mockEventService.Setup(s => s.GetAllAvailableEventsAsync()).ReturnsAsync(events);

            var controller = new SponsorshipController(
                _mockSponsorshipService.Object,
                _mockUserManager.Object,
                _mockPaymentService.Object,
                _mockStripePaymentService.Object,
                _mockEventService.Object);

            
            var result = await controller.SponsorEventList();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ExtendedEventViewModel>>(viewResult.Model);

            
            Assert.Equal(2, model.Count);

            Assert.Equal(1, model[0].Id);
            Assert.Equal("Event 1", model[0].Name);
            Assert.Equal("Venue 1", model[0].Venue);
            Assert.Equal("Description 1", model[0].Description);
            Assert.Equal("Organizer1", model[0].OrganizerEmail);

            Assert.Equal(2, model[1].Id);
            Assert.Equal("No Name Available", model[1].Name); 
            Assert.Equal("No Venue Assigned", model[1].Venue); 
            Assert.Equal("No Description Available", model[1].Description); 
            Assert.Equal("No Contact Info", model[1].OrganizerEmail); 
        }

    }
}
