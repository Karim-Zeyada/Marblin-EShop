using Marblin.Core.Entities;
using Marblin.Core.Enums;

namespace Marblin.Core.Specifications
{
    public class ProductsFilterSpecification : BaseSpecification<Product>
    {
        public ProductsFilterSpecification(string? search, int? categoryId, ProductAvailability? availability, string? sort)
            : base(p => 
                (string.IsNullOrEmpty(search) || p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search))) &&
                (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
                (!availability.HasValue || p.Availability == availability.Value)
            )
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Images); // Note: Original was .Images.Where(i => i.IsPrimary) - Specification include constraints are tricky in EF < 5, but EF Core supports filtered includes via string or ThenInclude. 
            // BaseSpecification supports basic Includes. For filtered includes, we might need to rely on the repository or custom string includes if BaseSpecification isn't extended for ThenInclude.
            // Simplified: Include all images for now, or use string include if strictly needed. 
            // Better: AddInclude("Images") and let EF Core handle it? No, filtered includes are better in code.
            // But BaseSpecification only has List<Expression<Func<T, object>>>.
            // Let's include Variants too.
            AddInclude(p => p.Variants);

            // Sorting
            switch (sort)
            {
                case "price_asc":
                    ApplyOrderBy(p => p.BasePrice);
                    break;
                case "price_desc":
                    ApplyOrderByDescending(p => p.BasePrice);
                    break;
                case "newest":
                    ApplyOrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    ApplyOrderByDescending(p => p.IsSignaturePiece);
                    // ThenByDescending(p => p.CreatedAt); // BaseSpecification doesn't typically support ThenBy without extension. 
                    // We'll stick to primary sort for now or extend IsSignaturePiece logic if critical.
                    break;
            }

            // Caching
            // Key depends on all parameters!
            var key = $"products_{search ?? "all"}_{categoryId}_{availability}_{sort ?? "def"}";
            EnableCache(key, TimeSpan.FromMinutes(10));
        }
    }
}
