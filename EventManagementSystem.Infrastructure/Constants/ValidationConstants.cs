using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Infrastructure.Constants
{
    public static class ValidationConstants
    {

        public const int NameMaxLength = 100;
        public const int AddressMaxLength = 250;
        public const int FeedbackMaxLength = 500;


        public const int CardNumberLength = 16;
        public const int CVVLength = 3;
        public const int ExpiryDateLength = 5;


        public const string RequiredField = "This field is required.";
        public const string MaxLengthError = "The {0} must not exceed {1} characters.";
        public const string InvalidEmail = "Please enter a valid email address.";
        public const string InvalidCardNumber = "The card number must be exactly 16 digits.";
        public const string InvalidCVV = "The CVV must be exactly 3 digits.";
        public const string InvalidExpiryDate = "The expiry date must be in the format MM/YY.";
        public const string InvalidPhoneNumber = "The phone number format is invalid.";
    }

}
