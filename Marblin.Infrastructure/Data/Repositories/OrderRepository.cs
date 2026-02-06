using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Infrastructure.Data.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : base(context, cache)
        {
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Set<Order>()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Set<Order>()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(OrderStatus? status, string? search, bool descending = true)
        {
            var query = _context.Set<Order>()
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => o.OrderNumber.Contains(search) || o.Email.Contains(search) || o.CustomerName.Contains(search));
            }

            if (descending)
            {
                query = query.OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                query = query.OrderBy(o => o.CreatedAt);
            }

            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(
            OrderStatus? status, string? search, int page = 1, int pageSize = 10, bool descending = true)
        {
            var query = _context.Set<Order>()
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => o.OrderNumber.Contains(search) || o.Email.Contains(search) || o.CustomerName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            if (descending)
            {
                query = query.OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                query = query.OrderBy(o => o.CreatedAt);
            }

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Marblin.Core.Models.OrderFinancials> GetDailyFinancialsAsync(DateTime date)
        {
            var dayOrders = _context.Set<Order>().Where(o => o.CreatedAt.Date == date.Date);

            return new Marblin.Core.Models.OrderFinancials
            {
                Revenue = await dayOrders.SumAsync(o => o.TotalAmount),
                Deposits = await dayOrders.Where(o => o.IsDepositVerified).SumAsync(o => o.DepositAmount)
            };
        }

        public async Task<List<Marblin.Core.Models.TopProductStat>> GetTopSellingItemsAsync(int count)
        {
            return await _context.Set<OrderItem>()
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => oi.ProductName)
                .Select(g => new Marblin.Core.Models.TopProductStat
                {
                    ProductName = g.Key,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<decimal>> GetRecentRevenueTrendAsync(int days)
        {
            var startDate = DateTime.Today.AddDays(-(days - 1));
            
            var revenueData = await _context.Set<Order>()
                .Where(o => o.CreatedAt >= startDate && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            var result = new List<decimal>();
            for (int i = 0; i < days; i++)
            {
                var targetDate = startDate.AddDays(i);
                var dayRevenue = revenueData.FirstOrDefault(r => r.Date == targetDate)?.Total ?? 0;
                result.Add(dayRevenue);
            }
            return result;
        }
    }
}
