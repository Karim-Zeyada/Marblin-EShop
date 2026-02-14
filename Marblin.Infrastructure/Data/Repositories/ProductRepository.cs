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

        public async Task<PaginatedList<Product>> GetProductsAsync(string? search, int? categoryId, ProductAvailability? availability, string? sort = null, int pageIndex = 1, int pageSize = 9, bool? onSale = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (availability.HasValue)
            {
                query = query.Where(p => p.Availability == availability.Value);
            }

            if (onSale.HasValue && onSale.Value)
            {
                var now = DateTime.UtcNow;
                query = query.Where(p => p.SalePrice != null && p.SalePrice < p.BasePrice 
                                      && (!p.SaleStartDate.HasValue || p.SaleStartDate <= now) 
                                      && (!p.SaleEndDate.HasValue || p.SaleEndDate > now));
            }

            // Apply default active filter
            query = query.Where(p => p.IsActive);

            query = sort switch
            {
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "price_asc" => query.OrderBy(p => (p.SalePrice != null && p.SalePrice < p.BasePrice && (!p.SaleStartDate.HasValue || p.SaleStartDate <= DateTime.UtcNow) && (!p.SaleEndDate.HasValue || p.SaleEndDate > DateTime.UtcNow)) ? p.SalePrice : p.BasePrice),
                "price_desc" => query.OrderByDescending(p => (p.SalePrice != null && p.SalePrice < p.BasePrice && (!p.SaleStartDate.HasValue || p.SaleStartDate <= DateTime.UtcNow) && (!p.SaleEndDate.HasValue || p.SaleEndDate > DateTime.UtcNow)) ? p.SalePrice : p.BasePrice),
                _ => query.OrderByDescending(p => p.IsFeaturedSale).ThenByDescending(p => p.IsSignaturePiece).ThenBy(p => p.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<Product>(items, count, pageIndex, pageSize);
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
             return await _context.Set<Product>()
                .Include(p => p.Category)

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

        public async Task<IEnumerable<Product>> GetRecommendationsForCartAsync(List<int> productIdsInCart, int count = 4)
        {
            var recommendations = new List<Product>();

            if (productIdsInCart.Any())
            {
                var cartCategories = await _context.Products
                    .Where(p => productIdsInCart.Contains(p.Id))
                    .Select(p => p.CategoryId)
                    .Distinct()
                    .ToListAsync();

                if (cartCategories.Any())
                {
                    // Fetch recommendations from these categories, randomizing order
                    recommendations = await _context.Products
                        .Include(p => p.Images.Where(i => i.IsPrimary))
                        .Where(p => cartCategories.Contains(p.CategoryId) && !productIdsInCart.Contains(p.Id) && p.IsActive)
                        .OrderBy(p => Guid.NewGuid()) 
                        .Take(count)
                        .ToListAsync();
                }
            }

            if (!recommendations.Any())
            {
                recommendations = await _context.Products
                    .Include(p => p.Images.Where(i => i.IsPrimary))
                    .Where(p => !productIdsInCart.Contains(p.Id) && p.IsActive && p.IsSignaturePiece)
                    .Take(count)
                    .ToListAsync();
            }

            return recommendations;
        }
    }
}
