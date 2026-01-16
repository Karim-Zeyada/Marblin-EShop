using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Marblin.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Infrastructure.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : base(context, cache)
        {
        }

        public async Task<PaginatedList<Product>> GetProductsAsync(string? search, int? categoryId, ProductAvailability? availability, string? sort = null, int pageIndex = 1, int pageSize = 9)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (availability.HasValue)
            {
                query = query.Where(p => p.Availability == availability.Value);
            }

            // Apply default active filter
            query = query.Where(p => p.IsActive);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.BasePrice),
                _ => query.OrderBy(p => p.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<Product>(items, count, pageIndex, pageSize);
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
             return await _context.Set<Product>()
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int categoryId, int excludeProductId, int count = 4)
        {
            return await _context.Set<Product>()
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .Where(p => p.CategoryId == categoryId && p.Id != excludeProductId && p.IsActive)
                .Take(count)
                .ToListAsync();
        }
    }
}
