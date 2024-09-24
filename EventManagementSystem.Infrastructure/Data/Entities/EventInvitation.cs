using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventManagementSystem.Infrastructure.Data.Enums;

namespace EventManagementSystem.Infrastructure.Data.Entities
{
    public class EventInvitation
    {
            public int Id { get; set; }

            //[Required]
            public string? SenderId { get; set; }
            [ForeignKey(nameof(SenderId))]
            public ApplicationUser? Sender { get; set; }

            
            public string ReceiverId { get; set; }
            [ForeignKey(nameof(ReceiverId))]
            public ApplicationUser? Receiver { get; set; }

            [Required]
            public int EventId { get; set; }
            [ForeignKey(nameof(EventId))]
            public Event? Event { get; set; }

            public DateTime InvitationDate { get; set; }
            public InvitationStatus Status { get; set; }
       
    }
}
