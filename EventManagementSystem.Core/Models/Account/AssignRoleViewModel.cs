using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Account
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public string SelectedRole { get; set; }
    }
}
