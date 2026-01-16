using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Application.Interfaces;
using Marblin.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IUnitOfWork _unitOfWork;

        public CartController(ICartService cartService, IUnitOfWork unitOfWork)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            
            var productIdsInCart = cart.Items.Select(i => i.ProductId).ToList();

            var recommendations = new List<Product>();

            if (productIdsInCart.Any())
            {
                 // Fetch categories of current cart items
                 var cartCategories = await _unitOfWork.Repository<Product>().Query()
                     .Where(p => productIdsInCart.Contains(p.Id))
                     .Select(p => p.CategoryId)
                     .Distinct()
                     .ToListAsync();
                 
                 if (cartCategories.Any())
                 {
                     recommendations = await _unitOfWork.Repository<Product>().Query()
                         .Include(p => p.Images)
                         .Where(p => cartCategories.Contains(p.CategoryId) && !productIdsInCart.Contains(p.Id) && p.IsActive)
                         .OrderBy(r => Guid.NewGuid()) // Randomize
                         .Take(4)
                         .ToListAsync();
                 }
            }

            // If no recommendations (empty cart or no related items), show Signature pieces or popular items
            if (!recommendations.Any())
            {
                recommendations = await _unitOfWork.Repository<Product>().Query()
                    .Include(p => p.Images)
                    .Where(p => !productIdsInCart.Contains(p.Id) && p.IsActive && p.IsSignaturePiece)
                    .Take(4)
                    .ToListAsync();
            }

            ViewBag.Recommendations = recommendations;
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int? variantId)
        {
            var product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            var variant = variantId.HasValue 
                ? product.Variants.FirstOrDefault(v => v.Id == variantId.Value) 
                : null;

            if (variantId.HasValue && variant == null) return NotFound();

            var price = variant?.GetFinalPrice(product.BasePrice) ?? product.BasePrice;
            var imageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
                         ?? product.Images.FirstOrDefault()?.ImageUrl;
            
            var item = new CartItem
            {
                ProductId = product.Id,
                VariantId = variantId,
                ProductName = product.Name,
                VariantDescription = variant != null ? $"{variant.Material} - {variant.Size}" : null,
                ImageUrl = imageUrl,
                UnitPrice = price,
                Quantity = 1 // Default to 1 for now
            };

            _cartService.AddItem(item);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId, int? variantId)
        {
            _cartService.RemoveItem(productId, variantId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }
    }
}
