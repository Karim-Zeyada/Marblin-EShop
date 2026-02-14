using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Application.Interfaces;
using Marblin.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductRepository _productRepository;

        public CartController(ICartService cartService, IProductRepository productRepository)
        {
            _cartService = cartService;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            var productIdsInCart = cart.Items.Select(i => i.ProductId).ToList();

            var recommendations = await _productRepository.GetRecommendationsForCartAsync(productIdsInCart);

            ViewBag.Recommendations = recommendations;
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int? variantId)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(productId);

            if (product == null) return NotFound();

            var variant = variantId.HasValue 
                ? product.Variants.FirstOrDefault(v => v.Id == variantId.Value) 
                : null;

            if (variantId.HasValue && variant == null) return NotFound();

            var isOnSale = product.IsOnSale();
            var activePrice = product.GetActivePrice();
            var price = variant?.GetFinalPrice(activePrice, isOnSale) ?? activePrice;
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
