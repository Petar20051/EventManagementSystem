using EventManagementSystem.Core.Models;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Core.Models.Home;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Core.Models.Reservation;
using EventManagementSystem.Core.Models.Sponsorship;
using EventManagementSystem.Core.Models.Venue;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests
{
    public class ViewModelsTests
    {
        [Theory]
        [InlineData("PaymentViewModel", "UserId", "user-123")]
        [InlineData("PaymentViewModel", "ReservationId", 1)]
       
        [InlineData("PaymentViewModel", "PaymentFor", PaymentFor.Ticket)]
        public void PaymentViewModel_ShouldSetAndGetProperties(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "PaymentViewModel" => new PaymentViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }
        [Fact]
        public void PaymentDetailsViewModel_CanBeInitialized()
        {
            
            var viewModel = new PaymentDetailsViewModel
            {
                PaymentId = 1,
                Amount = 100.50m,
                PaymentDate = new DateTime(2024, 12, 5),
                PaymentMethod = "Credit Card",
                Status = "Completed"
            };

            
            Assert.Equal(1, viewModel.PaymentId);
            Assert.Equal(100.50m, viewModel.Amount);
            Assert.Equal(new DateTime(2024, 12, 5), viewModel.PaymentDate);
            Assert.Equal("Credit Card", viewModel.PaymentMethod);
            Assert.Equal("Completed", viewModel.Status);
        }

        [Fact]
        public void PaymentDetailsViewModel_DefaultValuesAreUnset()
        {
            
            var viewModel = new PaymentDetailsViewModel();

            
            Assert.Equal(0, viewModel.PaymentId);
            Assert.Equal(0m, viewModel.Amount);
            Assert.Equal(default(DateTime), viewModel.PaymentDate);
            Assert.Null(viewModel.PaymentMethod);
            Assert.Null(viewModel.Status);
        }

        [Theory]
        [InlineData(1, 250.75, "2024-12-01", "PayPal", "Pending")]
        [InlineData(2, 500.00, "2024-11-30", "Credit Card", "Completed")]
        public void PaymentDetailsViewModel_SetProperties_CorrectValuesAreSet(
            int paymentId,
            decimal amount,
            string paymentDate,
            string paymentMethod,
            string status)
        {
            
            var date = DateTime.Parse(paymentDate);

            
            var viewModel = new PaymentDetailsViewModel
            {
                PaymentId = paymentId,
                Amount = amount,
                PaymentDate = date,
                PaymentMethod = paymentMethod,
                Status = status
            };

            
            Assert.Equal(paymentId, viewModel.PaymentId);
            Assert.Equal(amount, viewModel.Amount);
            Assert.Equal(date, viewModel.PaymentDate);
            Assert.Equal(paymentMethod, viewModel.PaymentMethod);
            Assert.Equal(status, viewModel.Status);
        }



        [Theory]
        [InlineData("EventFeedbackViewModel", "EventId", 123)]
        [InlineData("EventFeedbackViewModel", "EventName", "Sample Event")]
        [InlineData("EventFeedbackViewModel", "Feedbacks", null)] 
        [InlineData("EventFeedbackViewModel", "NewFeedback", null)] 
        public void EventFeedbackViewModel_ShouldSetAndGetProperties(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "EventFeedbackViewModel" => new FeedbackViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void EventFeedbackViewModel_ShouldInitializeWithDefaultValues()
        {
            
            var viewModel = new FeedbackViewModel();

            
            Assert.NotNull(viewModel.Feedbacks);
            Assert.Empty(viewModel.Feedbacks);
            Assert.NotNull(viewModel.NewFeedback);
        }
        [Theory]
        [InlineData("VenueViewModel", "Id", 1)]
        [InlineData("VenueViewModel", "Name", "Sample Venue")]
        public void VenueViewModel_ShouldSetAndGetProperties(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "VenueViewModel" => new VenueViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Theory]
        [InlineData("SponsorshipDashboardViewModel", "CurrentTier", SponsorshipTier.Gold)]
        [InlineData("SponsorshipDashboardViewModel", "Benefits", null)] 
        public void SponsorshipDashboardViewModel_ShouldSetAndGetProperties(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "SponsorshipDashboardViewModel" => new SponsorshipDashboardViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void SponsorshipDashboardViewModel_ShouldInitializeWithDefaultValues()
        {
            
            var viewModel = new SponsorshipDashboardViewModel();

            
            Assert.NotNull(viewModel.Benefits);
            Assert.Empty(viewModel.Benefits); 
        }

        [Fact]
        public void GetBenefits_ShouldReturnListOfSponsorshipBenefits()
        {
            
            var benefits = SponsorshipBenefits.GetBenefits();

            
            Assert.NotNull(benefits);
            Assert.Equal(3, benefits.Count);

            Assert.Equal(SponsorshipTier.Bronze, benefits[0].Tier);
            Assert.Equal("Discounts for tickets or event-related services", benefits[0].Description);
            Assert.Equal(1, benefits[0].MinimumSponsorshipAmount);

            Assert.Equal(SponsorshipTier.Silver, benefits[1].Tier);
            Assert.Equal("Logo displayed in the event brochure and other marketing materials", benefits[1].Description);
            Assert.Equal(20, benefits[1].MinimumSponsorshipAmount);

            Assert.Equal(SponsorshipTier.Gold, benefits[2].Tier);
            Assert.Equal("Logo in brochures + Social media shoutout + VIP seating", benefits[2].Description);
            Assert.Equal(100, benefits[2].MinimumSponsorshipAmount);
        }

        [Theory]
        [InlineData("SponsorPaymentViewModel", "EventId", 123)]
       
        [InlineData("SponsorPaymentViewModel", "SelectedCardId", "card-123")]
        [InlineData("SponsorPaymentViewModel", "SavedCards", null)] 
        [InlineData("SponsorPaymentViewModel", "CardNumber", "4242424242424242")]
        [InlineData("SponsorPaymentViewModel", "ExpiryMonth", "12")]
        [InlineData("SponsorPaymentViewModel", "ExpiryYear", "2030")]
        [InlineData("SponsorPaymentViewModel", "Cvc", "123")]
        public void SponsorPaymentViewModel_ShouldSetAndGetProperties(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "SponsorPaymentViewModel" => new SponsorPaymentViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void SponsorPaymentViewModel_ShouldInitializeWithDefaultValues()
        {
            
            var viewModel = new SponsorPaymentViewModel();

            
            Assert.NotNull(viewModel.SavedCards);
            Assert.Empty(viewModel.SavedCards); 
        }

        [Fact]
        public void SponsorPaymentViewModel_ShouldRequireAmountToBeGreaterThanZero()
        {
            
            var viewModel = new SponsorPaymentViewModel { Amount = 0 };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("Amount"));
            Assert.Contains(results, r => r.ErrorMessage == "Amount must be greater than zero.");
        }

       

        [Theory]
       
        [InlineData("SponsorPaymentViewModel", "SelectedCardId", "card-123")]
       
        [InlineData("SponsorPaymentViewModel", "EventId", 123)]
        public void SponsorPaymentViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "SponsorPaymentViewModel" => new SponsorPaymentViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void SponsorPaymentViewModel_DefaultValues_ShouldInitializeCorrectly()
        {
            
            var viewModel = new SponsorPaymentViewModel();

            
            Assert.NotNull(viewModel.SavedCards);
            Assert.Empty(viewModel.SavedCards); 
        }

       
       

        

        [Theory]
        [InlineData("ReservationViewModel", "Id", 123)]
        [InlineData("ReservationViewModel", "EventId", 456)]
        [InlineData("ReservationViewModel", "EventName", "Sample Event")]
        [InlineData("ReservationViewModel", "AvailableCapacity", 50)]
        [InlineData("ReservationViewModel", "AttendeesCount", 10)]
        [InlineData("ReservationViewModel", "ReservationDate", "2024-12-25")]
        [InlineData("ReservationViewModel", "Events", null)]
        [InlineData("ReservationViewModel", "IsPaid", true)]
        
        public void ReservationViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "ReservationViewModel" => new ReservationViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            if (propertyName == "ReservationDate" && value is string)
            {
                value = DateTime.Parse((string)value);
            }

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void ReservationViewModel_DefaultValues_ShouldInitializeCorrectly()
        {
            
            var viewModel = new ReservationViewModel();

            
            Assert.NotNull(viewModel.Events);
            Assert.Empty(viewModel.Events); 
        }

        [Fact]
        public void ReservationViewModel_Validation_ShouldRequireEventId()
        {
            
            var viewModel = new ReservationViewModel { EventId = null };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("EventId"));
            Assert.Contains(results, r => r.ErrorMessage == "Please select an event.");
        }

        [Fact]
        public void ReservationViewModel_Validation_ShouldRequireAttendeesCount()
        {
            
            var viewModel = new ReservationViewModel { AttendeesCount = 0 };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("AttendeesCount"));
            Assert.Contains(results, r => r.ErrorMessage == "Attendees count must be at least 1.");
        }

       
        [Theory]
        [InlineData("StoredPaymentMethodViewModel", "Id", 123)]
        [InlineData("StoredPaymentMethodViewModel", "CardHolderName", "John Doe")]
        [InlineData("StoredPaymentMethodViewModel", "Last4Digits", "1234")]
        [InlineData("StoredPaymentMethodViewModel", "ExpirationDate", "12/2030")]
        public void StoredPaymentMethodViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "StoredPaymentMethodViewModel" => new StoredPaymentMethodViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Theory]
        [InlineData("ProcessPaymentViewModel", "ReservationId", 123)]
        [InlineData("ProcessPaymentViewModel", "PaymentMethodID", "pm_12345")]
        [InlineData("ProcessPaymentViewModel", "SelectedCardId", "card_12345")]
        [InlineData("ProcessPaymentViewModel", "StoredCards", null)]
        public void ProcessPaymentViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "ProcessPaymentViewModel" => new ProcessPaymentViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void ProcessPaymentViewModel_DefaultValues_ShouldInitializeCorrectly()
        {
            
            var viewModel = new ProcessPaymentViewModel();

            
            Assert.NotNull(viewModel.StoredCards);
        }
        [Theory]
        [InlineData("PaymentMethodViewModel", "CardNumber", "4242424242424242")]
        [InlineData("PaymentMethodViewModel", "CVV", "123")]
        public void PaymentMethodViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "PaymentMethodViewModel" => new PaymentMethodViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void PaymentMethodViewModel_Validation_ShouldRequireCardNumber()
        {
            
            var viewModel = new PaymentMethodViewModel
            {
                CardNumber = null,
                ExpirationMonth = 12,
                ExpirationYear = 2030,
                CVV = "123"
            };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("CardNumber"));
        }

        

        

        [Fact]
        public void PaymentMethodViewModel_Validation_ShouldRequireCVV()
        {
            
            var viewModel = new PaymentMethodViewModel
            {
                CardNumber = "4242424242424242",
                ExpirationMonth = 12,
                ExpirationYear = 2030,
                CVV = null
            };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("CVV"));
        }
        [Theory]
        [InlineData("PaymentMethodInputModel", "PaymentMethodId", "pm_12345")]
        public void PaymentMethodInputModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "PaymentMethodInputModel" => new PaymentMethodInputModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

      
        [Theory]
        [InlineData("CardViewModel", "CardId", "card_12345")]
        [InlineData("CardViewModel", "CardHolderName", "John Doe")]
        [InlineData("CardViewModel", "Last4Digits", "4242")]
        [InlineData("CardViewModel", "ExpirationDate", "12/2030")]
        public void CardViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "CardViewModel" => new CardViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }
        [Fact]
        public void HomePageViewModel_ShouldInitializeWithEmptyUpcomingEvents()
        {
            
            var viewModel = new HomePageViewModel();

            
            Assert.NotNull(viewModel.UpcomingEvents);
            Assert.Empty(viewModel.UpcomingEvents); 
        }

        [Fact]
        public void HomePageViewModel_ShouldSetUpcomingEventsCorrectly()
        {
            
            var viewModel = new HomePageViewModel();
            var events = new List<Event>
        {
            new Event { Id = 1, Name = "Event 1", Date = DateTime.UtcNow.AddDays(1) },
            new Event { Id = 2, Name = "Event 2", Date = DateTime.UtcNow.AddDays(2) }
        };

            
            viewModel.UpcomingEvents = events;

            
            Assert.Equal(2, viewModel.UpcomingEvents.Count);
            Assert.Equal("Event 1", viewModel.UpcomingEvents[0].Name);
            Assert.Equal("Event 2", viewModel.UpcomingEvents[1].Name);
        }

        [Theory]
        [InlineData("ExtendedEventViewModel", "Id", 123)]
        [InlineData("ExtendedEventViewModel", "Name", "Sample Event")]
        [InlineData("ExtendedEventViewModel", "Date", "2024-12-25")]
        [InlineData("ExtendedEventViewModel", "Venue", "Sample Venue")]
        [InlineData("ExtendedEventViewModel", "Address", "123 Sample St.")]
        [InlineData("ExtendedEventViewModel", "Description", "This is a sample event.")]
        [InlineData("ExtendedEventViewModel", "OrganizerEmail", "organizer@example.com")]
        public void ExtendedEventViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "ExtendedEventViewModel" => new ExtendedEventViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            if (propertyName == "Date" && value is string)
            {
                value = DateTime.Parse((string)value);
            }

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }
        [Theory]
        [InlineData("EventViewModel", "Id", 123)]
        [InlineData("EventViewModel", "Name", "Sample Event")]
        [InlineData("EventViewModel", "Description", "This is a sample event.")]
        [InlineData("EventViewModel", "Date", "2024-12-25")]
        [InlineData("EventViewModel", "Location", "Sample Location")]
        [InlineData("EventViewModel", "OrganizerId", "organizer-123")]
        [InlineData("EventViewModel", "VenueId", 1)]
        [InlineData("EventViewModel", "ImageUrl", "https://example.com/event.jpg")]
        [InlineData("EventViewModel", "Venues", null)] 
        public void EventViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "EventViewModel" => new EventViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            if (propertyName == "Date" && value is string)
            {
                value = DateTime.Parse((string)value);
            }

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void EventViewModel_ShouldInitializeWithDefaultValues()
        {
            
            var viewModel = new EventViewModel();

            
            Assert.NotNull(viewModel.Venues);
            Assert.Empty(viewModel.Venues); 
        }

        [Theory]
        [InlineData("EventDetailsViewModel", "Id", 123)]
        [InlineData("EventDetailsViewModel", "Name", "Sample Event")]
        [InlineData("EventDetailsViewModel", "Description", "This is a detailed description of the event.")]
        [InlineData("EventDetailsViewModel", "Date", "2024-12-25")]
        [InlineData("EventDetailsViewModel", "Location", "Sample Location")]
        [InlineData("EventDetailsViewModel", "Address", "123 Sample St.")]
        [InlineData("EventDetailsViewModel", "OrganizerId", "organizer-123")]
        [InlineData("EventDetailsViewModel", "VenueId", 1)]
        public void EventDetailsViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "EventDetailsViewModel" => new EventDetailsViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            if (propertyName == "Date" && value is string)
            {
                value = DateTime.Parse((string)value);
            }

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void EventDetailsViewModel_ShouldInitializeWithNullValues()
        {
            
            var viewModel = new EventDetailsViewModel();

            
            Assert.Null(viewModel.Name);
            Assert.Null(viewModel.Description);
            Assert.Null(viewModel.Location);
            Assert.Null(viewModel.Address);
            Assert.Null(viewModel.OrganizerId);
            Assert.Null(viewModel.VenueId);
        }
        [Theory]
        [InlineData("CreateEventViewModel", "Name", "Sample Event")]
        [InlineData("CreateEventViewModel", "Description", "This is a sample event.")]
        [InlineData("CreateEventViewModel", "EventType", EventTypes.Concert)] 
        [InlineData("CreateEventViewModel", "Date", "2024-12-25")]
        [InlineData("CreateEventViewModel", "VenueId", 1)]  
        [InlineData("CreateEventViewModel", "ImageURL", "https://example.com/event.jpg")]
        [InlineData("CreateEventViewModel", "Venues", null)] 
        public void CreateEventViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "CreateEventViewModel" => new CreateEventViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            if (propertyName == "Date" && value is string)
            {
                value = DateTime.Parse((string)value);
            }

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void CreateEventViewModel_ShouldInitializeWithDefaultValues()
        {
            
            var viewModel = new CreateEventViewModel();

            
            Assert.NotNull(viewModel.Venues);
            Assert.Empty(viewModel.Venues); 
        }

        [Theory]
        [InlineData("AttendeeInfo", "UserEmail", "test@example.com")]
        [InlineData("AttendeeInfo", "AttendeeCount", 5)]
        public void AttendeeInfo_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "AttendeeInfo" => new AttendeeInfo(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }
        [Theory]
        [InlineData("ManageProfileViewModel", "UserName", "TestUser")]
        [InlineData("ManageProfileViewModel", "Email", "test@example.com")]
        [InlineData("ManageProfileViewModel", "PhoneNumber", "+1234567890")]
       
        [InlineData("ManageProfileViewModel", "Tickets", null)] 
        public void ManageProfileViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "ManageProfileViewModel" => new ManageProfileViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void ManageProfileViewModel_Validation_ShouldRequireEmail()
        {
            
            var viewModel = new ManageProfileViewModel { Email = null };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
            Assert.Contains(results, r => r.ErrorMessage == "The Email field is required.");
        }

        [Fact]
        public void ManageProfileViewModel_Validation_ShouldRequireValidEmail()
        {
            
            var viewModel = new ManageProfileViewModel { Email = "invalid-email" };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
            Assert.Contains(results, r => r.ErrorMessage == "The Email field is not a valid e-mail address.");
        }

        [Fact]
        public void ManageProfileViewModel_Validation_ShouldAllowPhoneNumber()
        {
            
            var viewModel = new ManageProfileViewModel { PhoneNumber = "+1234567890" };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
        }

        [Theory]
        [InlineData("ChangePasswordViewModel", "CurrentPassword", "oldPassword123")]
        [InlineData("ChangePasswordViewModel", "NewPassword", "newPassword123")]
        [InlineData("ChangePasswordViewModel", "ConfirmPassword", "newPassword123")]
        public void ChangePasswordViewModel_SetAndGetProperties_ShouldWorkCorrectly(string viewModelType, string propertyName, object value)
        {
            
            object viewModel = viewModelType switch
            {
                "ChangePasswordViewModel" => new ChangePasswordViewModel(),
                _ => throw new ArgumentException("Unknown ViewModel type")
            };

            var property = viewModel.GetType().GetProperty(propertyName);
            Assert.NotNull(property);

            
            property.SetValue(viewModel, value);
            var result = property.GetValue(viewModel);

            
            Assert.Equal(value, result);
        }

        [Fact]
        public void ChangePasswordViewModel_Validation_ShouldRequireCurrentPassword()
        {
            
            var viewModel = new ChangePasswordViewModel { CurrentPassword = null };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("CurrentPassword"));
            Assert.Contains(results, r => r.ErrorMessage == "Current Password is required.");
        }

        [Fact]
        public void ChangePasswordViewModel_Validation_ShouldRequireNewPassword()
        {
            
            var viewModel = new ChangePasswordViewModel { NewPassword = null };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("NewPassword"));
            Assert.Contains(results, r => r.ErrorMessage == "New Password is required.");
        }

        [Fact]
        public void ChangePasswordViewModel_Validation_ShouldRequireValidNewPasswordLength()
        {
            
            var viewModel = new ChangePasswordViewModel { NewPassword = "123" }; 
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("NewPassword"));
            Assert.Contains(results, r => r.ErrorMessage == "Password must be at least 6 characters long.");
        }

        [Fact]
        public void ChangePasswordViewModel_Validation_ShouldRequireConfirmPassword()
        {
            
            var viewModel = new ChangePasswordViewModel { ConfirmPassword = null };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
            Assert.Contains(results, r => r.ErrorMessage == "Please confirm your new password.");
        }

        [Fact]
        public void ChangePasswordViewModel_Validation_ShouldMatchNewPasswordAndConfirmPassword()
        {
            
            var viewModel = new ChangePasswordViewModel { NewPassword = "newPassword123", ConfirmPassword = "differentPassword123" };
            var context = new ValidationContext(viewModel);
            var results = new List<ValidationResult>();

            
            var isValid = Validator.TryValidateObject(viewModel, context, results, true);

            
            Assert.False(isValid); 
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
            Assert.Contains(results, r => r.ErrorMessage == "The new password and confirmation password do not match.");
        }
    }
}
