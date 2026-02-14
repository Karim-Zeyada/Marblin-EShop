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
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(productId);

            if (product == null) return NotFound();

            var price = product.IsOnSale() ? product.SalePrice ?? product.BasePrice : product.BasePrice;
            var imageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
                         ?? product.Images.FirstOrDefault()?.ImageUrl;
            
            var item = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = imageUrl,
                UnitPrice = price,
                Quantity = 1
            };

            _cartService.AddItem(item);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveItem(productId);
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
