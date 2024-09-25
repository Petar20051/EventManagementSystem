using EventManagementSystem.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EventMaganementSystem.Controllers
{
    [Route("Calendar")]
    public class CalendarController : Controller
    {
        private readonly IEventService _eventService; 

        public CalendarController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsAsync();
            ViewData["events"] = events;
            return View();
        }
    }

}
