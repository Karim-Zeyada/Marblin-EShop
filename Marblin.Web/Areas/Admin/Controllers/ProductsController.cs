using Marblin.Web.Areas.Admin.Models;
using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Marblin.Core.Enums;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// CRUD operations for products using Repository Pattern.
    /// </summary>
    public class ProductsController : AdminBaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileService _fileService;

        public ProductsController(IProductRepository productRepository, IUnitOfWork unitOfWork, IWebHostEnvironment environment, IFileService fileService)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _environment = environment;
            _fileService = fileService;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string? search, int? categoryId, ProductAvailability? availability)
        {
            // Use Repository for query encapsulation
            var products = await _productRepository.GetProductsAsync(search, categoryId, availability);

            ViewBag.Categories = await _unitOfWork.Repository<Category>().FindAsync(c => c.IsActive);
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Availability = availability;

            return View(products);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View(new ProductCreateViewModel());
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                BasePrice = model.BasePrice,
                CategoryId = model.CategoryId,
                IsSignaturePiece = model.IsSignaturePiece,
                Availability = model.Availability,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _productRepository.Add(product);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Product created successfully!";
            return RedirectToAction(nameof(Edit), new { id = product.Id });
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Use Repository specific method
            var product = await _productRepository.GetProductWithDetailsAsync(id);

            if (product == null)
                return NotFound();

            await LoadCategoriesAsync();

            var model = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                CategoryId = product.CategoryId,
                IsSignaturePiece = product.IsSignaturePiece,
                Availability = product.Availability,
                IsActive = product.IsActive,
                Variants = product.Variants.ToList(),
                Images = product.Images.ToList()
            };

            return View(model);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                // Need to reload variants/images if validation fails. 
                // Could call GetProductWithDetailsAsync again but we just need lists.
                // For simplicity, re-fetching whole product details or just sub-lists:
                model.Variants = (List<ProductVariant>)await _unitOfWork.Repository<ProductVariant>().FindAsync(v => v.ProductId == id);
                model.Images = (List<ProductImage>)await _unitOfWork.Repository<ProductImage>().FindAsync(i => i.ProductId == id);
                return View(model);
            }

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.BasePrice = model.BasePrice;
            product.CategoryId = model.CategoryId;
            product.IsSignaturePiece = model.IsSignaturePiece;
            product.Availability = model.Availability;
            product.IsActive = model.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // POST: Admin/Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            if (product == null)
                return NotFound();

            // 1. Nullify FK references in OrderItems to avoid constraint violation
            //    (OrderItem.ProductId and OrderItem.VariantId are nullable for this reason)
            var orderItems = await _unitOfWork.Repository<OrderItem>()
                .FindAsync(oi => oi.ProductId == id);
            foreach (var item in orderItems)
            {
                item.ProductId = null;
                item.VariantId = null;
            }

            // 2. Delete associated images and their files
            foreach (var image in product.Images)
            {
                _fileService.DeleteFile(image.ImageUrl);
            }

            // 3. Remove product (variants and images cascade-delete automatically)
            _productRepository.Remove(product);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Products/AddVariant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(int productId, string material, string size, decimal priceAdjustment, int stock, string? sku)
        {
            var variant = new ProductVariant
            {
                ProductId = productId,
                Material = material,
                Size = size,
                PriceAdjustment = priceAdjustment,
                Stock = stock,
                SKU = sku,
                IsActive = true
            };

            _unitOfWork.Repository<ProductVariant>().Add(variant);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Variant added!";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // POST: Admin/Products/DeleteVariant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int id, int productId)
        {
            var variant = await _unitOfWork.Repository<ProductVariant>().GetByIdAsync(id);
            if (variant != null)
            {
                _unitOfWork.Repository<ProductVariant>().Remove(variant);
                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Variant deleted!";
            }
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // POST: Admin/Products/UploadImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(int productId, IFormFile image, string? altText, bool isPrimary)
        {
            if (image == null || image.Length == 0)
            {
                TempData["Error"] = "Please select an image.";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            using var stream = image.OpenReadStream();
            var relativePath = await _fileService.SaveFileAsync(stream, image.FileName, FileCategory.ProductImage);

            // If setting as primary, unset other primaries
            if (isPrimary)
            {
                var existingPrimary = await _unitOfWork.Repository<ProductImage>()
                    .FindAsync(i => i.ProductId == productId && i.IsPrimary);
                foreach (var img in existingPrimary)
                    img.IsPrimary = false;
            }

            var count = await _unitOfWork.Repository<ProductImage>().CountAsync(i => i.ProductId == productId);

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = relativePath,
                AltText = altText,
                IsPrimary = isPrimary,
                SortOrder = count
            };

            _unitOfWork.Repository<ProductImage>().Add(productImage);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Image uploaded!";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // POST: Admin/Products/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id, int productId)
        {
            var image = await _unitOfWork.Repository<ProductImage>().GetByIdAsync(id);
            if (image != null)
            {
                _fileService.DeleteFile(image.ImageUrl);

                _unitOfWork.Repository<ProductImage>().Remove(image);
                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Image deleted!";
            }
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        private async Task LoadCategoriesAsync()
        {
            ViewBag.Categories = new SelectList(
                await _unitOfWork.Repository<Category>().FindAsync(c => c.IsActive),
                "Id", "Name");
        }
    }
}
