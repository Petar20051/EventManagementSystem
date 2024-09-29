using PayPal.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using PayPal;
using System.Globalization;

public class PayPalPaymentService : IPayPalPaymentService
{
    private readonly APIContext _apiContext;

    public PayPalPaymentService(IConfiguration configuration)
    {
        var clientId = configuration["PayPal:ClientId"];
        var clientSecret = configuration["PayPal:ClientSecret"];
        var oauthTokenCredential = new OAuthTokenCredential(clientId, clientSecret);
        var accessToken = oauthTokenCredential.GetAccessToken();

        _apiContext = new APIContext(accessToken)
        {
            Config = new Dictionary<string, string>
                {
                    { "mode", "sandbox" } // Change to "live" in production
                }
        };
    }

    public async Task<Payment> CreatePayPalPaymentAsync(decimal amount, string currency, string userId, int reservationId, PaymentFor paymentFor)
    {
        // Ensure that the amount is valid and currency is correct
        amount = 1;
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var payment = new Payment
        {
            intent = "sale",
            payer = new Payer { payment_method = "paypal" },
            transactions = new List<Transaction>
        {
            new Transaction
            {
                description = $"Payment for reservation {reservationId}",
                invoice_number = Guid.NewGuid().ToString(),
                amount = new Amount
                {
                    currency = currency,
                    total = amount.ToString("F2", CultureInfo.InvariantCulture) // Correctly format the amount
                }
            }
        },
            redirect_urls = new RedirectUrls
            {
                return_url = "http://your-return-url.com", // Add your return URL
                cancel_url = "http://your-cancel-url.com"  // Add your cancel URL
            }
        };

        try
        {
            var createdPayment = await Task.Run(() => payment.Create(_apiContext));
            return createdPayment;
        }
        catch (PaymentsException ex)
        {
            Console.WriteLine($"Error creating payment: {ex.Message}");
            Console.WriteLine($"Response: {ex.Response}");
            
            throw;
        }
    }
}

