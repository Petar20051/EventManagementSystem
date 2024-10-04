using Microsoft.AspNetCore.Mvc;
using EventManagementSystem.Infrastructure.Builders;
using EventManagementSystem.Infrastructure.Data.Entities;
using PayPalCheckoutSdk.Orders;

namespace EventMaganementSystem.Controllers
{


    public class PayPalCheckoutController : Controller
    {
        public async Task<IActionResult> CreateOrder()
        {
            var orderRequest = PayPalOrderBuilder.BuildOrder();
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(orderRequest);

            var response = await PayPalClient.Client().Execute(request);
            var result = response.Result<Order>();

            return Ok(result.Id); // return the order ID to the frontend
        }

        public async Task<IActionResult> CaptureOrder(string orderId)
        {
            var request = new OrdersCaptureRequest(orderId);
            request.RequestBody(new OrderActionRequest());
            var response = await PayPalClient.Client().Execute(request);
            var result = response.Result<Order>();

            return Ok(result);
        }
    }

}
