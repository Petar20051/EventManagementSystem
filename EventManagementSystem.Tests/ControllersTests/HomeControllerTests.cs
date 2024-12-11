using EventMaganementSystem.Controllers;
using EventMaganementSystem.Models;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Home;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockEventService = new Mock<IEventService>();
        }

   
    }
}
