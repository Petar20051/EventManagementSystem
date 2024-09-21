using EventManagementSystem.Infrastructure.Constants;
using EventManagementSystem.Infrastructure.Data.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace EventManagementSystem.Infrastructure.Entities
{
    public class ApplicationUser: IdentityUser
    {

        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public SponsorshipTier? SponsorshipTier { get; set; }  

        public decimal SponsoredAmount { get; set; }
    }
}
