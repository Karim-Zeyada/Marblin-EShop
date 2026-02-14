using Marblin.Core.Entities;

namespace Marblin.Core.Specifications
{
    public class FeaturedSaleProductsSpecification : BaseSpecification<Product>
    {
        public FeaturedSaleProductsSpecification(int take)
            : base(p => p.IsFeaturedSale && p.IsActive && p.SalePrice.HasValue)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            ApplyOrderByDescending(p => p.CreatedAt);
            ApplyPaging(0, take);
        }
    }
}
