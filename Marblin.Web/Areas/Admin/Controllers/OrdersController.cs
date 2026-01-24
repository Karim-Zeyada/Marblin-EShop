using Marblin.Web.Areas.Admin.Models;
using Marblin.Core.Interfaces;
using Marblin.Application.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index(OrderStatus? status, string? search)
        {
            // Use Repository specific method for complex filtering
            var orders = await _orderRepository.GetOrdersAsync(status, search);

            ViewBag.Status = status;
            ViewBag.Search = search;
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
            await _orderService.VerifyDepositAsync(id);

            TempData["Success"] = "Deposit verified!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatus);

            TempData["Success"] = $"Order status updated to {newStatus}!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
