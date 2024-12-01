
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
        public string? EventName { get; set; } = string.Empty; 
        public List<Feedback>? Feedbacks { get; set; } = new List<Feedback>(); 
        public Feedback? NewFeedback { get; set; } = new Feedback();
    }
}
