using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Web.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork; // Kept for Categories for now, or could move to Repo too

        public CatalogController(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int? categoryId, string? sort, int? page)
        {
            const int pageSize = 9;
            int pageIndex = page ?? 1;

            // Use specific repository method
            var products = await _productRepository.GetProductsAsync(null, categoryId, null, sort, pageIndex, pageSize);

            if (categoryId.HasValue)
            {
                ViewBag.CurrentCategory = await _unitOfWork.Repository<Category>().GetByIdAsync(categoryId.Value);
            }

            ViewBag.Categories = await _unitOfWork.Repository<Category>()
                .ListAsync(new Marblin.Core.Specifications.CachedCategorySpecification());
            
            ViewBag.Sort = sort;
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Use specific repository method
            var product = await _productRepository.GetProductWithDetailsAsync(id);

            if (product == null || !product.IsActive) return NotFound();

            // Use specific repository method
            ViewBag.RelatedProducts = await _productRepository.GetRelatedProductsAsync(product.CategoryId, id);

            return View(product);
        }
    }
}
