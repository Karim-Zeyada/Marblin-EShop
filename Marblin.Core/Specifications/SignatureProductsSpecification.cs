using Marblin.Core.Entities;

namespace Marblin.Core.Specifications
{
    public class SignatureProductsSpecification : BaseSpecification<Product>
    {
        public SignatureProductsSpecification(int take)
            : base(p => p.IsSignaturePiece && p.IsActive)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            ApplyOrderByDescending(p => p.CreatedAt);
            ApplyPaging(0, take);
        }
    }
}
