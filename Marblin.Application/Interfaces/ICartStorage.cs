using Marblin.Core.Models;

namespace Marblin.Application.Interfaces
{
    /// <summary>
    /// Abstraction for cart persistence mechanism.
    /// Implemented by Web layer (Session) or other storage providers.
    /// </summary>
    public interface ICartStorage
    {
        /// <summary>
        /// Retrieves the current shopping cart from storage.
        /// </summary>
        ShoppingCart GetCart();

        /// <summary>
        /// Persists the shopping cart to storage.
        /// </summary>
        void SaveCart(ShoppingCart cart);

        /// <summary>
        /// Clears the cart from storage.
        /// </summary>
        void ClearCart();
    }
}
