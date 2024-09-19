using EventManagementSystem.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string HolderId { get; set; }
        [ForeignKey(nameof(HolderId))]
        public ApplicationUser Holder { get; set; }
    }
}