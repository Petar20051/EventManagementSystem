﻿using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Core.Models.Sponsorship;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class SponsorshipController : Controller
    {
        private readonly ISponsorshipService _sponsorshipService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IEventService _eventService;

        public SponsorshipController(ISponsorshipService sponsorshipService, UserManager<ApplicationUser> userManager,
                                     IPaymentService paymentService, IStripePaymentService stripePaymentService,IEventService eventService)
        {
            _sponsorshipService = sponsorshipService;
            _userManager = userManager;
            _paymentService = paymentService;
            _stripePaymentService = stripePaymentService;
            _eventService = eventService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessSponsorship(ProcessSponsorshipViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model is null in POST");
            }

            if (string.IsNullOrEmpty(model.SelectedCardId))
            {
                ModelState.AddModelError("", "A payment method must be selected.");
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User is not authenticated.");
                return View(model);
            }

            
            var paymentStatus = await _stripePaymentService.ProcessSponsorshipPaymentAsync(model.Amount??0, model.SelectedCardId, userId);

            if (paymentStatus == "succeeded")
            {
                
                var user = await _userManager.FindByIdAsync(userId);
                user.SponsoredAmount += model.Amount??0;

                
                await _sponsorshipService.UpdateSponsorshipTierAsync(user, _userManager);

                
                var payment = new Payment
                {
                    UserId = userId,
                    Amount = model.Amount??0,
                    PaymentMethod = "Stripe Sponsorship", 
                    PaymentDate = DateTime.Now,
                    Status = "Completed"
                    
                };

                await _paymentService.RecordPaymentAsync(payment);

                TempData["Message"] = $"Thank you for your sponsorship! You are now a {user.SponsorshipTier} sponsor.";
                return RedirectToAction("SponsorshipDashboard");
            }
            else
            {
                ModelState.AddModelError("", "Payment failed. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProcessSponsorship(int eventId)
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User is not authenticated.";
                return Redirect("/Identity/Account/Login");
            }

            
            var savedCards = await _stripePaymentService.GetStoredCardsAsync(userId);

            
            var model = new ProcessSponsorshipViewModel
            {
                EventId = eventId,
                Cards = savedCards.Select(card => new CardViewModel
                {
                    CardId = card.CardId,
                    Last4Digits = card.Last4Digits
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SponsorshipDashboard()
        {

            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userid == null) return Redirect("/Identity/Account/Login");
            var user = await _userManager.FindByIdAsync(userid);
             

            var model = new SponsorshipDashboardViewModel
            {
                CurrentTier = user.SponsorshipTier ?? SponsorshipTier.None,
                SponsoredAmount = user.SponsoredAmount,
                Benefits = SponsorshipBenefits.GetBenefits()
                    .Where(b => b.Tier == user.SponsorshipTier)
                    .ToList()
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> SponsorEventList()
        {
            
            var events = await _eventService.GetAllAvailableEventsAsync();

            
            var model = events.Select(e => new ExtendedEventViewModel
            {
                Id = e.Id,
                Name = e.Name ?? "No Name Available", 
                Date = e.Date,
                Venue = e.Venue?.Name ?? "No Venue Assigned", 
                Description = e.Description ?? "No Description Available",
                OrganizerEmail = e.Organizer.UserName ?? "No Contact Info"
            }).ToList();

            return View(model);
        }
    }
}

