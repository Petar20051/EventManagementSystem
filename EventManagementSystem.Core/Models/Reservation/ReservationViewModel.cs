using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Web.Mvc;
using EventManagementSystem.Infrastructure.Entities;



namespace EventManagementSystem.Core.Models.Reservation
{
    public class ReservationViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        public int? EventId { get; set; }

        public string? EventName { get; set; }

        public int? AvailableCapacity { get; set; }

        [Required(ErrorMessage = "Please enter the number of attendees.")]
        [Range(1, int.MaxValue, ErrorMessage = "Attendees count must be at least 1.")]
        public int AttendeesCount { get; set; }

        [Required(ErrorMessage = "Please enter a reservation date.")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        public IEnumerable<Event> Events { get; set; } = new List<Event>();

        
        public bool IsPaid { get; set; } 

        public decimal TotalAmount { get; set; }
        public decimal? DiscountedAmount { get; set; }
    }
}
