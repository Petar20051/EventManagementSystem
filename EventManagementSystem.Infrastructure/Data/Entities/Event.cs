using EventManagementSystem.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [MaxLength(ValidationConstants.NameMaxLength, ErrorMessage = ValidationConstants.MaxLengthError)]
        public string Name { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime Date { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int VenueId { get; set; } // Foreign key for Venue

        [ForeignKey(nameof(VenueId))]
        public Venue Venue { get; set; } // Navigation property to Venue

        public string OrganizerId { get; set; }

        [ForeignKey(nameof(OrganizerId))]
        public ApplicationUser Organizer { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
