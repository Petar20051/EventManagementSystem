using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EventMaganementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{code}")]
        public IActionResult HttpStatusCodeHandler(int code)
        {
            ViewData["ErrorCode"] = code;

            switch (code)
            {
                case 404:
                    ViewData["ErrorMessage"] = "The page you are looking for could not be found.";
                    break;
                case 403:
                    ViewData["ErrorMessage"] = "You do not have permission to access this resource.";
                    break;
                case 500:
                    ViewData["ErrorMessage"] = "An internal server error occurred. Please try again later.";
                    break;
                default:
                    ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                    break;
            }

            return View("HttpStatusCode");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exceptionDetails?.Error is InvalidOperationException invalidOperationEx)
            {
                ViewData["ErrorMessage"] = invalidOperationEx.Message;
                ViewData["ErrorCode"] = 500; 
            }
            else
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                ViewData["ErrorCode"] = 500;
            }

            return View("Error");
        }
    }
}
