using Marblin.Core.Entities;

namespace Marblin.Web.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel for Admin Dashboard.
    /// </summary>
    public class DashboardViewModel
    {
        // Today's Stats
        public int TodayOrdersCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal TodayDeposits { get; set; }

        // Orders by Status
        public int PendingDepositCount { get; set; }
        public int InProductionCount { get; set; }
        public int AwaitingBalanceCount { get; set; }
        public int ShippedCount { get; set; }
        public int CancelledCount { get; set; }

        public int TotalActiveOrders => PendingDepositCount + InProductionCount + AwaitingBalanceCount;

        // Recent Orders
        public List<Order> RecentOrders { get; set; } = new();

        // Custom Requests
        public int UnreviewedRequestsCount { get; set; }

        // Products
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Analytics
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartData { get; set; } = new();
        public List<ProductSalesInfo> TopSellingProducts { get; set; } = new();
    }

    public class ProductSalesInfo
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
