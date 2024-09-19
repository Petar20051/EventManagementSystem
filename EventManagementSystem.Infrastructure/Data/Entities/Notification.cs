using EventManagementSystem.Infrastructure.Constants;
using EventManagementSystem.Infrastructure.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string Message { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime NotificationDate { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public NotificationType Type { get; set; }
    }
}
