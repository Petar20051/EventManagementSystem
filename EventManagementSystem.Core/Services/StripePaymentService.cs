using Microsoft.Extensions.Configuration;
using Stripe;
using EventManagementSystem.Core.Contracts;
using Stripe.Checkout;
using EventManagementSystem.Core.Models.Payments;
using Microsoft.Extensions.Options;
using EventManagementSystem.Core;
using EventManagementSystem.Core.Services;
using Microsoft.Extensions.Logging;
using EventManagementSystem.Core.Extensions;

public class StripePaymentService : IStripePaymentService
{
    private readonly IUserService _userService;
    private readonly StripeSettings _stripeOptions;
    private readonly IDiscountService _discountService;
    private readonly PaymentMethodService _paymentMethodService;





    public StripePaymentService(
        IUserService userService,
        IOptions<StripeSettings> stripeOptions,
        IDiscountService discountService)
    {
        _userService = userService;
        _stripeOptions = stripeOptions.Value;
        _discountService = discountService;
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        _paymentMethodService = new PaymentMethodService();
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
        var user= await _userService.GetUserByIdAsync(userId);

        if (string.IsNullOrEmpty(customerId))
        {
            throw new Exception("Stripe customer ID not found.");
        }
        decimal discountedAmount = await _discountService.ApplyDiscountAsync(user.SponsorshipTier,  amount);

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(discountedAmount * 100), // Convert amount to cents
            Currency = "usd", // Use your currency code here
            Customer = customerId,
            PaymentMethod = paymentMethodId,
            Confirm = true,
            PaymentMethodTypes = new List<string> { "card" }, // Specify the payment method types
            ReturnUrl = "https://localhost:7056/Payments/PaymentSuccess" // Add your return URL here
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return paymentIntent.Status;
    }
    public async Task<string> ProcessSponsorshipPaymentAsync(decimal amount, string paymentMethodId, string userId)
    {
        var customerId = await _userService.GetStripeCustomerIdAsync(userId);
        var user = await _userService.GetUserByIdAsync(userId);

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
            PaymentMethodTypes = new List<string> { "card" }, // Specify the payment method types
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
        var paymentMethodService = new PaymentMethodService();

        try
        {
            // Fetch the payment method details
            var paymentMethod = await paymentMethodService.GetAsync(paymentMethodId);

            // Check if the payment method is already attached
            if (paymentMethod.Customer?.Id == customerId)
            {
                Console.WriteLine($"Payment method {paymentMethodId} is already attached to customer {customerId}.");
                return; // Exit early if the card is already attached
            }

            if (!string.IsNullOrEmpty(paymentMethod.Customer?.Id))
            {
                throw new InvalidOperationException($"Payment method {paymentMethodId} is already attached to another customer.");
            }

            // Attach the payment method
            await paymentMethodService.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
            {
                Customer = customerId
            });

            Console.WriteLine($"Payment method {paymentMethodId} attached to customer {customerId} successfully.");
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Stripe Error: {ex.StripeError?.Message}");
            throw new InvalidOperationException($"Stripe error: {ex.StripeError?.Message}", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new InvalidOperationException("An error occurred while attaching the payment method.", ex);
        }
    }



    public async Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId)
    {
        var service = new PaymentMethodService();
        return await service.GetAsync(paymentMethodId);
    }

    public async Task UpdateCustomerDefaultPaymentMethodAsync(string customerId, string paymentMethodId)
    {
        var service = new CustomerService();
        await service.UpdateAsync(customerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        });
    }

   
}
