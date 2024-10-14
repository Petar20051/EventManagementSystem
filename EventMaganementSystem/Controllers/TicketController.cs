using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.IO;

namespace EventMaganementSystem.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(ITicketService ticketService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyTickets()
        {
            var userId = _userManager.GetUserId(User);

            var tickets = await _ticketService.GetUserTicketsAsync(userId);

            foreach (var ticket in tickets)
            {
                // Generate SVG QR code with ticket ID or event info
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(ticket.Id.ToString(), QRCodeGenerator.ECCLevel.Q);

                // Use SvgQRCode to generate SVG QR code
                SvgQRCode qrCode = new SvgQRCode(qrCodeData);
                string svgQrCode = qrCode.GetGraphic(5); // Reduced the scale factor for a smaller QR code

                // Assign the SVG to the ticket's QRCodeSvg property
                ticket.QRCodeSvg = svgQrCode; // Make sure the property name is QRCodeSvg in the Ticket class
            }

            return View(tickets);
        }
    }
}
