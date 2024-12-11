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
    [Route("EventInvitation")]
    public class EventInvitationController : Controller
    {
        private readonly IEventInvitationService _eventInvitationService;
        private readonly EventDbContext _context;

        public EventInvitationController(IEventInvitationService eventInvitationService, EventDbContext eventDbContext)
        {
            _eventInvitationService = eventInvitationService;
            _context = eventDbContext;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            ViewBag.UserId = userId;
            var invitations = await _eventInvitationService.GetAllInvitationsForUserAsync(userId);
            return View(invitations);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.EventList = new SelectList(_context.Events.ToList(), "Id", "Name");
            ViewBag.UserList = new SelectList(_context.Users.ToList(), "Id", "UserName");
            return View();
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(EventInvitation invitation)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventList = new SelectList(_context.Events.ToList(), "Id", "Name");
                ViewBag.UserList = new SelectList(_context.Users.ToList(), "Id", "UserName");
                return View(invitation);
            }

            if (!_context.Events.Any(e => e.Id == invitation.EventId) || !_context.Users.Any(u => u.Id == invitation.ReceiverId))
            {
                ModelState.AddModelError("", "Invalid event or receiver.");
                return View(invitation);
            }

            invitation.SenderId = GetUserId(User);
            if (string.IsNullOrEmpty(invitation.SenderId)) return Unauthorized();

           
            if (invitation.SenderId == invitation.ReceiverId)
            {
                TempData["ErrorMessage"]=( "You cannot send an invitation to yourself.");
                ViewBag.EventList = new SelectList(_context.Events.ToList(), "Id", "Name");
                ViewBag.UserList = new SelectList(_context.Users.ToList(), "Id", "UserName");
                return View(invitation);
            }

            await _eventInvitationService.SendInvitationAsync(invitation.SenderId, invitation.ReceiverId, invitation.EventId);
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var invitation = await _eventInvitationService.GetInvitationByIdAsync(id);
                if (invitation == null) return NotFound();

                return View(invitation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var invitation = await _eventInvitationService.GetInvitationByIdAsync(id);
            if (invitation == null) return NotFound();

            return View(invitation);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _eventInvitationService.DeleteInvitationAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to delete the invitation.");
            }
        }

        [HttpPost]
        [Route("Confirm/{id}")]
        public async Task<IActionResult> ConfirmInvitation(int id)
        {
            var invitation = await _eventInvitationService.GetInvitationByIdAsync(id);
            if (invitation == null) return NotFound();

            await _eventInvitationService.ConfirmInvitationAsync(id);
            return RedirectToAction("Index");
        }

        private string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

}

