using Marblin.Application.DTOs;
using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Models;

namespace Marblin.Application.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order from submission DTO and shopping cart.
        /// </summary>
        Task<Order> CreateOrderAsync(OrderSubmissionDto submission, ShoppingCart cart);

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

        /// <summary>
        /// Verifies the deposit for an order.
        /// </summary>
        Task VerifyDepositAsync(int orderId);
        
        /// <summary>
        /// Retrieves an order by its public order number.
        /// </summary>
        Task<Order?> GetOrderByNumberAsync(string orderNumber);

        /// <summary>
        /// Submits a payment proof file. Returns updated Order or null if not found.
        /// </summary>
        Task<Order?> SubmitPaymentProofAsync(int orderId, Stream fileStream, string fileName);

        /// <summary>
        /// Submits a payment proof transaction ID. Returns updated Order or null if not found.
        /// </summary>
        Task<Order?> SubmitPaymentProofAsync(int orderId, string transactionId);

        /// <summary>
        /// Submits a balance payment proof file. Returns updated Order or null if not found.
        /// </summary>
        Task<Order?> SubmitBalancePaymentProofAsync(int orderId, Stream fileStream, string fileName);

        /// <summary>
        /// Submits a balance payment proof transaction ID. Returns updated Order or null if not found.
        /// </summary>
        Task<Order?> SubmitBalancePaymentProofAsync(int orderId, string transactionId);

        /// <summary>
        /// Verifies the balance payment for an order.
        /// </summary>
        Task VerifyBalanceAsync(int orderId);

        /// <summary>
        /// Retrieves the current site settings.
        /// </summary>
        Task<SiteSettings?> GetSiteSettingsAsync();

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        Task<Order?> GetOrderByIdAsync(int id);
    }
}
