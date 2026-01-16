using Marblin.Core.Entities;

namespace Marblin.Core.Specifications
{
    public class CachedCategorySpecification : BaseSpecification<Category>
    {
        public CachedCategorySpecification(bool activeOnly = true)
            : base(c => !activeOnly || (c.IsActive && c.Products.Any()))
        {
            // Cache for 1 hour as categories change rarely
            EnableCache("categories_list", TimeSpan.FromMinutes(60));
        }
    }
}
