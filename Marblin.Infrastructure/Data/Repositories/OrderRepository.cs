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
    }
}
