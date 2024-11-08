using EventMaganementSystem.Models;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Home;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventMaganementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventService _eventService;

        public HomeController(ILogger<HomeController> logger,IEventService eventService)

        {
            _logger = logger;
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(); // Fetches the next 5 upcoming events by default

            var viewModel = new HomePageViewModel
            {
                UpcomingEvents = upcomingEvents
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
