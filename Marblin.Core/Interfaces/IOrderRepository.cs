using Marblin.Core.Entities;
using Marblin.Core.Enums;

namespace Marblin.Core.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<Order?> GetOrderWithItemsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersAsync(OrderStatus? status, string? search, bool descending = true);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(OrderStatus? status, string? search, int page = 1, int pageSize = 10, bool descending = true);
        
        // Dashboard Stats
        Task<Models.OrderFinancials> GetDailyFinancialsAsync(DateTime date);
        Task<List<Models.TopProductStat>> GetTopSellingItemsAsync(int count);
        Task<List<decimal>> GetRecentRevenueTrendAsync(int days);
    }
}
