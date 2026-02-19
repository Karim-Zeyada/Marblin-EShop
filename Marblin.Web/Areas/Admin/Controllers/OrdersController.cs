using Marblin.Web.Areas.Admin.Models;
using Marblin.Core.Interfaces;
using Marblin.Application.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Marblin.Core.Enums;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Order management with status workflow using Repository Pattern.
    /// </summary>
    public class OrdersController : AdminBaseController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrdersController(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(OrderStatus? status, string? search, int page = 1)
        {
            const int pageSize = 10;
            
            // Use Repository specific method for complex filtering with pagination
            var (orders, totalCount) = await _orderRepository.GetOrdersPagedAsync(status, search, page, pageSize);

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;
            
            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Use Repository specific method
            var order = await _orderRepository.GetOrderWithItemsAsync(id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Admin/Orders/VerifyDeposit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDeposit(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order != null)
                {
                    await _orderService.VerifyDepositAsync(id);
                    var msg = order.PaymentMethod == PaymentMethod.FullPaymentUpfront 
                        ? "Full payment verified!" 
                        : "Deposit verified!";
                    TempData["Success"] = msg;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, newStatus);
                TempData["Success"] = $"Order status updated to {newStatus}!";
            }
            catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Orders/VerifyBalance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyBalance(int id)
        {
            try
            {
                await _orderService.VerifyBalanceAsync(id);
                TempData["Success"] = "Balance verified!";
            }
            catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id, string? reason, bool isRefunded, decimal refundAmount)
        {
            try
            {
                await _orderService.CancelOrderAsync(id, reason ?? "Customer request", isRefunded, refundAmount);
                TempData["Success"] = "Order cancelled successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Orders/Refund/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefundOrder(int id, decimal amount)
        {
            try
            {
                await _orderService.RefundOrderAsync(id, amount);
                TempData["Success"] = "Order marked as refunded successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
