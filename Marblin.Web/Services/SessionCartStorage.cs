using Marblin.Application.Interfaces;
using Marblin.Core.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Marblin.Web.Services
{
    /// <summary>
    /// Session-based cart storage adapter.
    /// Implements ICartStorage using HTTP Session for persistence.
    /// </summary>
    public class SessionCartStorage : ICartStorage
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "MarblinCart";

        public SessionCartStorage(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ShoppingCart GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var json = session?.GetString(SessionKey);
            return json == null ? new ShoppingCart() : JsonConvert.DeserializeObject<ShoppingCart>(json) ?? new ShoppingCart();
        }

        public void SaveCart(ShoppingCart cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var json = JsonConvert.SerializeObject(cart);
            session?.SetString(SessionKey, json);
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }
    }
}
