
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models
{
    public class FeedbackViewModel
    {
        public int EventId { get; set; }
        public string? EventName { get; set; } = string.Empty; // Ensure default value.
        public List<Feedback>? Feedbacks { get; set; } = new List<Feedback>(); // Ensure an empty list.
        public Feedback? NewFeedback { get; set; } = new Feedback();
    }
}
