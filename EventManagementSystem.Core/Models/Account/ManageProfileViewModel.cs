using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Account
{
    public class ManageProfileViewModel
    {
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public decimal SponsoredAmount { get; set; }

        public List<Ticket> Tickets { get; set; }

        public string SponsorshipTier { get; set; }
    }
}
