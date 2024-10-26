using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaganementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Admin/SendGeneralNotification
        [HttpPost]
        public async Task<IActionResult> SendGeneralNotification(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("", "Message content is required.");
                return View("Index");
            }

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                await _notificationService.NotifyGeneralAnnouncementAsync(user.Id, message);
            }

            TempData["Message"] = "General notification sent successfully!";
            return RedirectToAction("Index");
        }

        // POST: /Admin/SendSystemAlert
        [HttpPost]
        public async Task<IActionResult> SendSystemAlert(string alertMessage)
        {
            if (string.IsNullOrEmpty(alertMessage))
            {
                ModelState.AddModelError("", "Alert content is required.");
                return View("Index");
            }

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                await _notificationService.NotifySystemAlertAsync(user.Id, alertMessage);
            }

            TempData["Message"] = "System alert sent successfully!";
            return RedirectToAction("Index");
        }
    }
}
