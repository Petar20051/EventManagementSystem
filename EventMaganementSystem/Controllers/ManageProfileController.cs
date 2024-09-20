using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Account;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class ManageProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly EventDbContext _context;

        public ManageProfileController(IProfileService profileService,EventDbContext context)
        {
            _profileService = profileService;
            _context = context;
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
                PhoneNumber=user.PhoneNumber
               
            };

            return View(model);
        }
        public string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
