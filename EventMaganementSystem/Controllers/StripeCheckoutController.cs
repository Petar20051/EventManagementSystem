using Stripe;
using EventManagementSystem.Core.Contracts;
using Stripe.Checkout;

public class StripePaymentService : IStripePaymentService
{
    private readonly string _secretKey;

    public StripePaymentService(IConfiguration config)
    {
        _secretKey = config["Stripe:SecretKey"];
    }

    public async Task<string> CreateCheckoutSession(decimal amount, int reservationId)
    {
        StripeConfiguration.ApiKey = _secretKey;

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = amount * 100, // Convert to cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Reservation #" + reservationId
                        },
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = "https://localhost:5001/Payments/PaymentSuccess",
            CancelUrl = "https://localhost:5001/Payments/PaymentFailed",
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Id;
    }
}
