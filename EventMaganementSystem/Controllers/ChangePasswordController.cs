using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Account;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class ChangePasswordController : Controller
    {
        private readonly IProfileService _profileService;

        public ChangePasswordController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var user = await _profileService.GetUserAsync(GetUserId(User));
            if (user == null) return NotFound("User not found");

            var model = new ChangePasswordViewModel();
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _profileService.GetUserAsync(GetUserId(User));
            if (user == null) return RedirectToAction("Index", "Home");

            var result = await _profileService.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded) return RedirectToAction("Index", "ManageProfile"); // Ensure this is correct

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        private string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}