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
    public class Discount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string SponsorId { get; set; }
        [ForeignKey(nameof(SponsorId))]
        public ApplicationUser Sponsor { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public decimal Percentage { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }
    }
}
