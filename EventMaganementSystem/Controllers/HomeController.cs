using EventMaganementSystem.Models;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Home;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventMaganementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger,IEventService eventService,UserManager<ApplicationUser> userManager)

        {
            _logger = logger;
            _eventService = eventService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch the upcoming events
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(); // Fetches the next 5 upcoming events by default

            // Check if the current user is an admin
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            // Create the view model with dynamic data
            var viewModel = new HomePageViewModel
            {
                UpcomingEvents = upcomingEvents,
                IsAdmin = isAdmin
            };

            return View(viewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
