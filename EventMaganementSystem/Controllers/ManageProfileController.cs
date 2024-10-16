using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class ManageProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly EventDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageProfileController(IProfileService profileService,EventDbContext context,UserManager<ApplicationUser> user)
        {
            _profileService = profileService;
            _context = context;
            _userManager = user;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _profileService.GetUserAsync(GetUserId(User));
            if (user == null) return NotFound("User not found");
            var tickets = _context.Tickets.Where(t=>t.HolderId==user.Id).ToList();
            var model = new ManageProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                SponsoredAmount = user.SponsoredAmount,
                SponsorshipTier = user.SponsorshipTier.ToString(),
                Tickets=tickets,
              
               
            };

            return View(model);
        }
        public string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BecomeOrganizer()
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if the user is already in the Organizer role
            if (!await _userManager.IsInRoleAsync(user, "Organizer"))
            {
                // Add user to the Organizer role
                var result = await _userManager.AddToRoleAsync(user, "Organizer");
                if (result.Succeeded)
                {
                    TempData["Message"] = "You are now an Organizer!";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle errors (optional)
                    TempData["Error"] = "Failed to assign Organizer role.";
                }
            }
            else
            {
                TempData["Error"] = "You are already an Organizer.";
            }

            return RedirectToAction("Index");
        }
    }
}
