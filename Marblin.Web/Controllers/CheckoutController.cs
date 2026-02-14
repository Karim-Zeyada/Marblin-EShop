using AutoMapper;
using Marblin.Application.DTOs;
using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Marblin.Application.Interfaces;
using Marblin.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public CheckoutController(ICartService cartService, IOrderService orderService, IMapper mapper)
        {
            _cartService = cartService;
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var settings = await _orderService.GetSiteSettingsAsync() 
                           ?? new SiteSettings { DepositPercentage = 50 };

            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                Settings = settings
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                model.Settings = await _orderService.GetSiteSettingsAsync() 
                                 ?? new SiteSettings { DepositPercentage = 50 };
                return View(model);
            }

            var submissionDto = _mapper.Map<OrderSubmissionDto>(model);

            // Create Order delegation
            var order = await _orderService.CreateOrderAsync(submissionDto, cart);

            _cartService.ClearCart();

            // NEW FLOW: Redirect to payment method selection instead of directly to confirmation
            return RedirectToAction(nameof(SelectPaymentMethod), new { id = order.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> SelectPaymentMethod(string id)
        {
            var order = await _orderService.GetOrderByNumberAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPaymentMethod(int orderId, int paymentMethod)
        {
            var method = (PaymentMethod)paymentMethod;
            var order = await _orderService.SetPaymentMethodAsync(orderId, method);
            
            if (order == null) return NotFound();

            return RedirectToAction(nameof(Confirmation), new { id = order.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(string id)
        {
            var order = await _orderService.GetOrderByNumberAsync(id);

            if (order == null) return NotFound();

            ViewBag.Settings = await _orderService.GetSiteSettingsAsync();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UploadPaymentProof(int orderId, IFormFile? receiptImage, string? transactionId)
        {
            Order? order = null;

            if (receiptImage != null && receiptImage.Length > 0)
            {
                using var stream = receiptImage.OpenReadStream();
                order = await _orderService.SubmitPaymentProofAsync(orderId, stream, receiptImage.FileName);
            }
            else if (!string.IsNullOrWhiteSpace(transactionId))
            {
                order = await _orderService.SubmitPaymentProofAsync(orderId, transactionId);
            }
            else
            {

                TempData["Error"] = "Please provide a receipt image or transaction ID.";
                
                order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return NotFound();
                
                return RedirectToAction(nameof(Confirmation), new { id = order.OrderNumber });
            }

            if (order == null) return NotFound();

            TempData["Success"] = "Payment proof submitted! We will verify it shortly.";
            return RedirectToAction(nameof(Confirmation), new { id = order.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> PayBalance(string id)
        {
            var order = await _orderService.GetOrderByNumberAsync(id);

            if (order == null) return NotFound();
            

            if (order.Status != OrderStatus.AwaitingBalance && order.Status != OrderStatus.InProduction) 
            {
                return RedirectToAction(nameof(Confirmation), new { id = order.OrderNumber });
            }

            ViewBag.Settings = await _orderService.GetSiteSettingsAsync();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UploadBalanceProof(int orderId, IFormFile? receiptImage, string? transactionId)
        {
            Order? order = null;

            if (receiptImage != null && receiptImage.Length > 0)
            {
                using var stream = receiptImage.OpenReadStream();
                order = await _orderService.SubmitBalancePaymentProofAsync(orderId, stream, receiptImage.FileName);
            }
            else if (!string.IsNullOrWhiteSpace(transactionId))
            {
                order = await _orderService.SubmitBalancePaymentProofAsync(orderId, transactionId);
            }
            else
            {
                TempData["Error"] = "Please provide a receipt image or transaction ID.";
                order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return NotFound();
                
                return RedirectToAction(nameof(PayBalance), new { id = order.OrderNumber });
            }

            if (order == null) return NotFound();

            TempData["Success"] = "Balance payment proof submitted! We will verify it shortly.";
            return RedirectToAction(nameof(Confirmation), new { id = order.OrderNumber });
        }
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "Please enter a coupon code.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _cartService.ApplyCouponAsync(code);
            if (success)
            {
                TempData["Success"] = "Coupon applied successfully!";
            }
            else
            {
                TempData["Error"] = "Invalid or expired coupon code.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            _cartService.RemoveCoupon();
            TempData["Success"] = "Coupon removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
