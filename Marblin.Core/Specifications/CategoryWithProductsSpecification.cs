using Marblin.Core.Entities;

namespace Marblin.Core.Specifications
{
    public class CategoryWithProductsSpecification : BaseSpecification<Category>
    {
        public CategoryWithProductsSpecification() 
        {
            AddInclude(c => c.Products);
            ApplyOrderBy(c => c.SortOrder);
        }

        public CategoryWithProductsSpecification(int id) 
            : base(c => c.Id == id)
        {
            AddInclude(c => c.Products);
        }
    }
}
