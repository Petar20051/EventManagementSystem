using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    [Authorize]
    public class EventInvitationController : Controller
    {
        private readonly IEventInvitationService _eventInvitationService;
        private readonly EventDbContext _context;

        public EventInvitationController(IEventInvitationService eventInvitationService, EventDbContext eventDbContext)
        {
            _eventInvitationService = eventInvitationService;
            _context = eventDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId(User); // Get the current user's ID
            ViewBag.UserId = userId; // Pass the UserId to the view
            var invitations = await _eventInvitationService.GetAllInvitationsForUserAsync(userId);
            return View(invitations);
        }

        public IActionResult Create()
        {
            ViewBag.EventList = new SelectList(_context.Events.ToList(), "Id", "Name");
            ViewBag.UserList = new SelectList(_context.Users.ToList(), "Id", "UserName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventInvitation invitation)
        {
            if (ModelState.IsValid)
            {
              
                invitation.SenderId = GetUserId(User);

                await _eventInvitationService.SendInvitationAsync(invitation.SenderId, invitation.ReceiverId, invitation.EventId);
                return RedirectToAction("Index");
            }

            ViewBag.EventList = new SelectList(_context.Events.ToList(), "Id", "Name");
            ViewBag.UserList = new SelectList(_context.Users.ToList(), "Id", "UserName");
            return View(invitation);
        }

        public async Task<IActionResult> Details(int id)
        {
            var invitation = await _eventInvitationService.GetInvitationByIdAsync(id); // Implement this in your service
            return View(invitation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var invitation = await _eventInvitationService.GetInvitationByIdAsync(id); // Implement this in your service
            return View(invitation);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _eventInvitationService.DeleteInvitationAsync(id); // Implement this in your service
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmInvitation(int id)
        {
            try
            {
                // Fetch the invitation before confirming, just for validation if needed
                var invitation = await _eventInvitationService.GetInvitationByIdAsync(id);
                if (invitation == null)
                {
                    return NotFound(); // Handle case where the invitation does not exist
                }

                await _eventInvitationService.ConfirmInvitationAsync(id); // Confirm the invitation
                return RedirectToAction("Index"); // Redirect to the list of invitations
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle any errors
            }
        }


        private string GetUserId(ClaimsPrincipal user)
        {
           var userId= user.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
