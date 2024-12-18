﻿using EventManagementSystem.Infrastructure.Constants;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Data.Enums;
using Newtonsoft.Json;
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

        [Required]
        public decimal TicketPrice { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int VenueId { get; set; } 

        [ForeignKey(nameof(VenueId))]
        public Venue Venue { get; set; } 

        public string OrganizerId { get; set; }

        [ForeignKey(nameof(OrganizerId))]
        public ApplicationUser Organizer { get; set; }

         public string? ImageUrl { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<EventInvitation> Invitations { get; set; } = new List<EventInvitation>();

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public EventTypes EventType { get; set; } 
    }
}