using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Common;

namespace Marblin.Core.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<PaginatedList<Product>> GetProductsAsync(string? search, int? categoryId, ProductAvailability? availability, string? sort = null, int pageIndex = 1, int pageSize = 9, bool? onSale = null);
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int categoryId, int excludeProductId, int count = 4);
        Task<IEnumerable<Product>> GetRecommendationsForCartAsync(List<int> productIdsInCart, int count = 4);
    }
}
