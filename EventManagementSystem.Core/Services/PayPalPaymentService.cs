using EventManagementSystem.Core.Contracts;
using Microsoft.Extensions.Configuration;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;

public class PayPalPaymentService : IPayPalPaymentService
{
    private readonly PayPalHttpClient _client;

    public PayPalPaymentService(IConfiguration config)
    {
        var environment = new SandboxEnvironment(config["PayPal:ClientId"], config["PayPal:Secret"]);
        _client = new PayPalHttpClient(environment);
    }

    public async Task<string> CreateOrderAsync(decimal amount, int reservationId)
    {
        var order = new OrderRequest
        {
            CheckoutPaymentIntent = "CAPTURE",
            PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = "USD",
                        Value = amount.ToString()
                    }
                }
            }
        };

        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(order);

        var response = await _client.Execute(request);
        var result = response.Result<Order>();

        return result.Id;
    }

    public async Task<bool> CaptureOrderAsync(string orderId)
    {
        var request = new OrdersCaptureRequest(orderId);
        request.RequestBody(new OrderActionRequest());
        var response = await _client.Execute(request);

        return response.StatusCode == System.Net.HttpStatusCode.Created;
    }
}
