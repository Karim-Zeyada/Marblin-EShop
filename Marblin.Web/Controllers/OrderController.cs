using Marblin.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Marblin.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult Track()
        {
            return View();
        }

        [HttpPost]
        [EnableRateLimiting("TrackingPolicy")]
        public async Task<IActionResult> Track(string orderNumber, string email)
        {
            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Please enter both Order ID and Email.");
                return View();
            }

            var order = await _orderService.GetOrderByNumberAsync(orderNumber);

            // Simple validation: check if email matches
            if (order == null || !order.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Order not found. Please check your details.");
                return View();
            }

            HttpContext.Session.SetString($"VerifiedOrder_{order.OrderNumber}", "true");
            return RedirectToAction(nameof(Details), new { id = order.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Require email-verified access via Track action (session-backed)
            var verified = HttpContext.Session.GetString($"VerifiedOrder_{id}");
            if (verified != "true")
            {
                return RedirectToAction(nameof(Track));
            }

            var order = await _orderService.GetOrderByNumberAsync(id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
