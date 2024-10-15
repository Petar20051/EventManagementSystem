using Microsoft.Extensions.Configuration;
using Stripe;
using EventManagementSystem.Core.Contracts;
using Stripe.Checkout;
using EventManagementSystem.Core.Models.Payments;
using Microsoft.Extensions.Options;
using EventManagementSystem.Core;

public class StripePaymentService : IStripePaymentService
{
    private readonly IUserService _userService;
    private readonly StripeSettings _stripeOptions;

    public StripePaymentService(IUserService userService, IOptions<StripeSettings> stripeOptions)
    {
        _userService = userService;
        _stripeOptions = stripeOptions.Value;

        // Set the Stripe secret key from configuration (user secrets)
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
    }

    public async Task AddPaymentMethodAsync(string stripeCustomerId, string stripeToken)
    {
        var paymentMethodService = new PaymentMethodService();

        // Attach the payment method to the customer using the token
        var attachOptions = new PaymentMethodAttachOptions
        {
            Customer = stripeCustomerId
        };

        await paymentMethodService.AttachAsync(stripeToken, attachOptions);
    }


    public async Task<string> CreateStripeCustomerAsync(string userId ,string email, string userName)
    {
        var options = new CustomerCreateOptions
        {
           
            Email = email,
            Name = userName
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        return customer.Id;
    }
    public async Task<string> ProcessPaymentAsync(decimal amount, string paymentMethodId, string userId)
    {
        var customerId = await _userService.GetStripeCustomerIdAsync(userId);

        if (string.IsNullOrEmpty(customerId))
        {
            throw new Exception("Stripe customer ID not found.");
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert amount to cents
            Currency = "usd", // Use your currency code here
            Customer = customerId,
            PaymentMethod = paymentMethodId,
            Confirm = true,
            ReturnUrl = "https://localhost:7056/Payments/PaymentSuccess" // Add your return URL here
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return paymentIntent.Status;
    }
    public async Task<List<CardViewModel>> GetStoredCardsAsync(string userId)
    {
        var customerId = await _userService.GetStripeCustomerIdAsync(userId);

        if (string.IsNullOrEmpty(customerId))
        {
            return new List<CardViewModel>();
        }

        var options = new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card"
        };

        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(options);

        return paymentMethods.Data.Select(pm => new CardViewModel
        {
            CardId = pm.Id,
            CardHolderName = pm.BillingDetails.Name,
            Last4Digits = pm.Card.Last4,
            ExpirationDate = $"{pm.Card.ExpMonth}/{pm.Card.ExpYear}"
        }).ToList();
    }
    public async Task<string> CreateSetupIntentAsync(string customerId)
    {
        var options = new SetupIntentCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new SetupIntentService();
        var setupIntent = await service.CreateAsync(options);

        return setupIntent.ClientSecret; // Use this on the client side to confirm the setup
    }
    public async Task AttachPaymentMethodAsync(string customerId, string paymentMethodId)
    {
        var service = new PaymentMethodService();
        var attachOptions = new PaymentMethodAttachOptions
        {
            Customer = customerId,  // Ensure the correct Stripe customer ID is passed
        };
        await service.AttachAsync(paymentMethodId, attachOptions);
    }
}
