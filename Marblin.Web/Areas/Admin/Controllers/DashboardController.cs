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
        private readonly IOrderRepository _orderRepo;

        public DashboardController(IUnitOfWork unitOfWork, IOrderRepository orderRepo)
        {
            _unitOfWork = unitOfWork;
            _orderRepo = orderRepo;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            
            var productRepo = _unitOfWork.Repository<Product>();
            var variantRepo = _unitOfWork.Repository<ProductVariant>();
            var customRequestRepo = _unitOfWork.Repository<CustomRequest>();

            var financials = await _orderRepo.GetDailyFinancialsAsync(today);
            var trend = await _orderRepo.GetRecentRevenueTrendAsync(7);
            var topProducts = await _orderRepo.GetTopSellingItemsAsync(5);

            var viewModel = new DashboardViewModel
            {
                TodayOrdersCount = await _orderRepo.CountAsync(o => o.CreatedAt.Date == today),
                TodayRevenue = financials.Revenue,
                TodayDeposits = financials.Deposits,

                PendingDepositCount = await _orderRepo.CountAsync(o => o.Status == OrderStatus.PendingDeposit),
                InProductionCount = await _orderRepo.CountAsync(o => o.Status == OrderStatus.InProduction),
                AwaitingBalanceCount = await _orderRepo.CountAsync(o => o.Status == OrderStatus.AwaitingBalance),
                ShippedCount = await _orderRepo.CountAsync(o => o.Status == OrderStatus.Shipped),

                RecentOrders = (await _orderRepo.GetOrdersAsync(null, null, true)).Take(5).ToList(),

                UnreviewedRequestsCount = await customRequestRepo.CountAsync(cr => !cr.IsReviewed),

                TotalProducts = await productRepo.CountAsync(p => true), 
                LowStockCount = await variantRepo.CountAsync(v => v.Stock > 0 && v.Stock <= 3),

                ChartLabels = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-6 + i).ToString("MMM dd")).ToList(),
                ChartData = trend
            };

            viewModel.TopSellingProducts = topProducts.Select(tp => new ProductSalesInfo 
            {
                ProductName = tp.ProductName,
                UnitsSold = tp.UnitsSold,
                Revenue = tp.Revenue
            }).ToList();

            return View(viewModel);
        }
    }
}
