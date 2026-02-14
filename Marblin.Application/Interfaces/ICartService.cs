using Marblin.Core.Models;

namespace Marblin.Application.Interfaces
{
    /// <summary>
    /// Cart use cases - defines business operations for shopping cart.
    /// </summary>
    public interface ICartService
    {
        ShoppingCart GetCart();
        void SaveCart(ShoppingCart cart);
        void AddItem(CartItem item);
        void RemoveItem(int productId);
        void ClearCart();
        
        Task<bool> ApplyCouponAsync(string code);
        void RemoveCoupon();
    }
}
