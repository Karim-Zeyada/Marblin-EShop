using Marblin.Application.Interfaces;
using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Core.Models;
using Marblin.Core.Specifications;

namespace Marblin.Application.Services
{
    /// <summary>
    /// Pure business logic for cart operations.
    /// No dependency on HTTP or any web framework.
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartStorage _cartStorage;
        private readonly IRepository<Coupon> _couponRepository;

        public CartService(ICartStorage cartStorage, IRepository<Coupon> couponRepository)
        {
            _cartStorage = cartStorage;
            _couponRepository = couponRepository;
        }

        public ShoppingCart GetCart()
        {
            return _cartStorage.GetCart();
        }

        public void SaveCart(ShoppingCart cart)
        {
            _cartStorage.SaveCart(cart);
        }

        public void AddItem(CartItem item)
        {
            var cart = GetCart();
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                cart.Items.Add(item);
            }
            
            // Recalculate discount if coupon exists?
            // For now, let's just save. If items change, total changes, but discount amount might be fixed or percentage.
            // If percentage, we need to recalculate discount amount.
            RecalculateDiscount(cart);

            SaveCart(cart);
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                RecalculateDiscount(cart);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            _cartStorage.ClearCart();
        }

        public async Task<bool> ApplyCouponAsync(string code)
        {
            var spec = new CouponByCodeSpecification(code);
            var coupon = await _couponRepository.GetEntityWithSpec(spec);

            if (coupon == null || !coupon.IsValid())
            {
                return false;
            }

            var cart = GetCart();
            cart.AppliedCouponCode = coupon.Code;
            cart.DiscountPercentage = coupon.DiscountPercentage;
            
            CalculateDiscount(cart, coupon);
            
            SaveCart(cart);
            return true;
        }

        public void RemoveCoupon()
        {
            var cart = GetCart();
            cart.AppliedCouponCode = null;
            cart.DiscountPercentage = null;
            cart.DiscountAmount = 0;
            SaveCart(cart);
        }

        private void RecalculateDiscount(ShoppingCart cart)
        {
            if (string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                cart.DiscountAmount = 0;
                return;
            }

            if (cart.DiscountPercentage.HasValue)
            {
                cart.DiscountAmount = cart.SubTotal * (cart.DiscountPercentage.Value / 100m);
            }
            // If it was a fixed amount, it remains fixed unless we have logic to re-verify it doesn't exceed total (capped below).
            
            // Cap at subtotal
            if (cart.DiscountAmount > cart.SubTotal)
            {
                cart.DiscountAmount = cart.SubTotal;
            }
        }
        
        private void CalculateDiscount(ShoppingCart cart, Coupon coupon)
        {
            if (coupon.DiscountPercentage.HasValue)
            {
                cart.DiscountAmount = cart.SubTotal * (coupon.DiscountPercentage.Value / 100m);
            }
            else if (coupon.DiscountAmount.HasValue)
            {
                cart.DiscountAmount = coupon.DiscountAmount.Value;
            }
            
            // Cap at subtotal
            if (cart.DiscountAmount > cart.SubTotal)
            {
                cart.DiscountAmount = cart.SubTotal;
            }
        }
    }
}
