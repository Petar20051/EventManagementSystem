using EventManagementSystem.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string UserId { get; set; }
        [ForeignKey(nameof (UserId))]
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [MaxLength(ValidationConstants.FeedbackMaxLength, ErrorMessage = ValidationConstants.MaxLengthError)]
        public string FeedbackContent { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime FeedbackDate { get; set; }
    }
}
