using Marblin.Web.Areas.Admin.Models;
using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marblin.Core.Enums;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin dashboard showing overview analytics.
    /// </summary>
    public class DashboardController : AdminBaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = today.AddDays(-30);

            var ordersQuery = _unitOfWork.Repository<Order>().Query();
            var customRequestsQuery = _unitOfWork.Repository<CustomRequest>().Query();
            var productsQuery = _unitOfWork.Repository<Product>().Query();
            var variantsQuery = _unitOfWork.Repository<ProductVariant>().Query();
            var orderItemsQuery = _unitOfWork.Repository<OrderItem>().Query();

            var viewModel = new DashboardViewModel
            {
                // Today's stats
                TodayOrdersCount = await ordersQuery
                    .CountAsync(o => o.CreatedAt.Date == today),
                TodayRevenue = await ordersQuery
                    .Where(o => o.CreatedAt.Date == today)
                    .SumAsync(o => o.TotalAmount),
                TodayDeposits = await ordersQuery
                    .Where(o => o.CreatedAt.Date == today && o.IsDepositVerified)
                    .SumAsync(o => o.DepositAmount),

                // Orders by status
                PendingDepositCount = await ordersQuery
                    .CountAsync(o => o.Status == OrderStatus.PendingDeposit),
                InProductionCount = await ordersQuery
                    .CountAsync(o => o.Status == OrderStatus.InProduction),
                AwaitingBalanceCount = await ordersQuery
                    .CountAsync(o => o.Status == OrderStatus.AwaitingBalance),
                ShippedCount = await ordersQuery
                    .CountAsync(o => o.Status == OrderStatus.Shipped),

                // Recent orders
                RecentOrders = await ordersQuery
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                // Unreviewed custom requests
                UnreviewedRequestsCount = await customRequestsQuery
                    .CountAsync(cr => !cr.IsReviewed),

                // Product stats
                TotalProducts = await productsQuery.CountAsync(),
                LowStockCount = await variantsQuery
                    .CountAsync(v => v.Stock > 0 && v.Stock <= 3)
            };

            // Chart Data (Last 7 Days Revenue)
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-6 + i)).ToList();
            viewModel.ChartLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList();
            viewModel.ChartData = new List<decimal>();

            var revenueData = await ordersQuery
                .Where(o => o.CreatedAt >= DateTime.Today.AddDays(-7) && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            foreach (var date in last7Days)
            {
                var dayRevenue = revenueData.FirstOrDefault(r => r.Date == date)?.Total ?? 0;
                viewModel.ChartData.Add(dayRevenue);
            }

            // Top Selling Products
            var topProductsRaw = await orderItemsQuery
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .ToListAsync(); // Client eval for GroupBy complexity if needed

            viewModel.TopSellingProducts = topProductsRaw
                .GroupBy(oi => oi.ProductName)
                .Select(g => new ProductSalesInfo
                {
                    ProductName = g.Key,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(5)
                .ToList();

            return View(viewModel);
        }
    }
}
