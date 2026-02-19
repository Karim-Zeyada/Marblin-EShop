using Marblin.Application.Interfaces;
using Marblin.Application.DTOs;
using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Marblin.Core.Models;
using Microsoft.Extensions.Logging;
using Marblin.Core.Specifications;
using Microsoft.Extensions.Configuration;

namespace Marblin.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IOrderFactory _orderFactory;
        private readonly ILogger<OrderService> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public OrderService(
            IUnitOfWork unitOfWork, 
            IFileService fileService, 
            IOrderFactory orderFactory,
            ILogger<OrderService> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _orderFactory = orderFactory;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<Order> CreateOrderAsync(OrderSubmissionDto model, ShoppingCart cart)
        {
            _logger.LogInformation("Creating order for customer {Email} with {ItemCount} items", 
                model.Email, cart.Items.Count);

            var spec = new SiteSettingsSpecification();
            var settings = await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);
            var depositPct = settings?.DepositPercentage ?? Marblin.Core.Constants.OrderConstants.DefaultDepositPercentage;

            // Use Factory for Order Creation logic (SRP)
            var order = _orderFactory.CreateOrder(model, cart, depositPct);

            try
            {
                _unitOfWork.Repository<Order>().Add(order);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Order {OrderNumber} created successfully.", order.OrderNumber);
                
                // Increment coupon usage if one was applied :)
                if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
                {
                    var couponSpec = new CouponByCodeSpecification(cart.AppliedCouponCode);
                    var coupon = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(couponSpec);
                    if (coupon != null)
                    {
                        coupon.TimesUsed++;
                        // Coupon usage incremented
                    }
                }

                // DECREMENT STOCK (Allow Negative / Backorder)
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductId.HasValue)
                    {
                        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId.Value);
                        if (product != null)
                        {
                            product.Stock -= item.Quantity;
                            if (product.Stock < 0)
                            {
                                _logger.LogWarning("Product {ProductName} (ID: {Id}) stock went negative: {Stock}. Order: {OrderNumber}", 
                                    product.Name, product.Id, product.Stock, order.OrderNumber);
                            }
                        }
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();
                
                // Email will be sent after payment method is selected in SetPaymentMethodAsync

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order for {Email}", model.Email);
                throw;
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            var oldStatus = order.Status;
            order.UpdateStatus(newStatus);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}", orderId, oldStatus, newStatus);

            // Handle specific status notification logic
            try
            {
                if (newStatus == OrderStatus.AwaitingBalance)
                {
                    // [FLOW 4] Awaiting Balance
                    var spec = new SiteSettingsSpecification();
                    var settings = await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);
                    
                    await _emailService.SendAwaitingBalanceEmailAsync(
                        order.Email,
                        order.CustomerName,
                        order.OrderNumber,
                        order.TotalAmount,
                        order.DepositAmount,
                        order.RemainingBalance,
                        settings?.InstapayAccount ?? "N/A",
                        settings?.VodafoneCashNumber ?? "N/A",
                        order.PaymentMethod,
                        order.City,
                        settings?.CairoGizaShippingCost ?? 0m);
                }
                else if (newStatus == OrderStatus.Shipped)
                {
                    // [FLOW 5] Shipped
                    await _emailService.SendOrderShippedEmailAsync(
                        order.Email,
                        order.CustomerName,
                        order.OrderNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send status update email for {OrderNumber} status {Status}", order.OrderNumber, newStatus);
            }
        }

        public async Task VerifyDepositAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            _logger.LogInformation("Verifying deposit for Order {OrderNumber}.", order.OrderNumber);
            order.VerifyDeposit();
            await _unitOfWork.SaveChangesAsync();
            
            // [FLOW 3] Deposit Verified -> Production
            try
            {
                await _emailService.SendDepositVerifiedEmailAsync(
                    order.Email,
                    order.CustomerName,
                    order.OrderNumber,
                    order.TotalAmount,
                    order.RemainingBalance,
                    order.PaymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send deposit verification email for {OrderNumber}", order.OrderNumber);
            }
        }
        
        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            var spec = new OrderWithItemsSpecification(orderNumber);
            return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<Order?> SubmitPaymentProofAsync(int orderId, Stream fileStream, string fileName)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return null;

            var relativePath = await _fileService.SaveFileAsync(fileStream, fileName, FileCategory.ReceiptImage);
            order.SetPaymentProof(relativePath, PaymentProofType.ReceiptImage);
            await _unitOfWork.SaveChangesAsync();

            // [FLOW 2] Proof Received
            await NotifyProofReceived(order);
            
            return order;
        }

        public async Task<Order?> SubmitPaymentProofAsync(int orderId, string transactionId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return null;

            order.SetPaymentProof(transactionId, PaymentProofType.TransactionId);
            await _unitOfWork.SaveChangesAsync();

            // [FLOW 2] Proof Received
            await NotifyProofReceived(order);
            
            return order;
        }

        public async Task<Order?> SubmitBalancePaymentProofAsync(int orderId, Stream fileStream, string fileName)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return null;

            var relativePath = await _fileService.SaveFileAsync(fileStream, fileName, FileCategory.ReceiptImage);
            order.SetBalancePaymentProof(relativePath, PaymentProofType.ReceiptImage);
            await _unitOfWork.SaveChangesAsync();

            // Optionally notify for balance proof as well, or use the same logic
            await NotifyProofReceived(order);
            
            return order;
        }

        public async Task<Order?> SubmitBalancePaymentProofAsync(int orderId, string transactionId)
        {
             var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
             if (order == null) return null;

             order.SetBalancePaymentProof(transactionId, PaymentProofType.TransactionId);
             await _unitOfWork.SaveChangesAsync();

             await NotifyProofReceived(order);
             
             return order;
        }

        private async Task NotifyProofReceived(Order order)
        {
            try
            {
                await _emailService.SendPaymentProofReceivedEmailAsync(order.Email, order.CustomerName, order.OrderNumber, order.PaymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment proof received email for {OrderNumber}", order.OrderNumber);
            }
        }

        public async Task VerifyBalanceAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            order.VerifyBalance();
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Balance verified for Order {OrderId}. Status: {Status}", orderId, order.Status);

            // [FLOW 5] Balance verified -> Shipped: send notification
            if (order.Status == OrderStatus.Shipped)
            {
                try
                {
                    await _emailService.SendOrderShippedEmailAsync(
                        order.Email,
                        order.CustomerName,
                        order.OrderNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send shipped email for {OrderNumber}", order.OrderNumber);
                }
            }
        }

        public async Task<Order?> SetPaymentMethodAsync(int orderId, PaymentMethod paymentMethod)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return null;

            order.PaymentMethod = paymentMethod;

            // Update Deposit Amount based on Payment Method
            if (paymentMethod == PaymentMethod.FullPaymentUpfront)
            {
                order.DepositAmount = order.TotalAmount;
            }
            else // CashOnDelivery
            {
                // Revert to percentage based deposit if user switches back
                order.DepositAmount = (order.TotalAmount * order.DepositPercentage) / 100m;
            }

            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Payment method set to {PaymentMethod} for Order {OrderNumber}. Deposit Updated: {DepositAmount}", 
                paymentMethod, order.OrderNumber, order.DepositAmount);
            
            // [FLOW 1] Send Confirmation Email AFTER payment method is selected
            try
            {
                var spec = new SiteSettingsSpecification();
                var settings = await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);
                
                var baseUrl = _configuration["EmailSettings:BaseUrl"] ?? "https://marblin.com";
                var proofUrl = $"{baseUrl}/Checkout/Confirmation/{order.OrderNumber}";
                
                await _emailService.SendOrderConfirmationEmailAsync(
                    order.Email, 
                    order.CustomerName, 
                    order.OrderNumber, 
                    order.TotalAmount, 
                    order.DepositAmount,
                    settings?.InstapayAccount ?? "N/A",
                    settings?.VodafoneCashNumber ?? "N/A",
                    proofUrl,
                    order.PaymentMethod,
                    order.City,
                    settings?.CairoGizaShippingCost ?? 0m);
                    
                _logger.LogInformation("Order confirmation email sent for {OrderNumber}", order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email for {OrderNumber}", order.OrderNumber);
            }
            
            return order;
        }

        public async Task<SiteSettings?> GetSiteSettingsAsync()
        {
            var spec = new SiteSettingsSpecification();
            return await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Order>().GetByIdAsync(id);
        }

        public async Task<Order?> CancelOrderAsync(int orderId, string reason, bool isRefunded, decimal refundAmount)
        {
            var spec = new OrderWithItemsSpecification(orderId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec); // Need items for stock reversal
            if (order == null) return null;
            
            // Note: 2-day cancellation window is enforced on the customer side only.
            // Admins can cancel orders at any time.
            
            // Validate current status
            if (order.Status == OrderStatus.Shipped)
            {
                throw new InvalidOperationException("Cannot cancel shipped orders.");
            }
            
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Order is already cancelled.");
            }
            
            _logger.LogInformation("Cancelling order {OrderNumber}. Reason: {Reason}", order.OrderNumber, reason);
            
            // Update order status
            order.UpdateStatus(OrderStatus.Cancelled);
            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = reason;

            // Handle Refund
            if (isRefunded)
            {
                try 
                {
                    order.MarkAsRefunded(refundAmount);
                    _logger.LogInformation("Order {OrderNumber} marked as refunded. Amount: {Amount}", order.OrderNumber, refundAmount);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Failed to mark order {OrderNumber} as refunded.", order.OrderNumber);
                }
            }
            
            // Reverse Stock
            foreach (var item in order.OrderItems)
            {
                if (item.ProductId.HasValue) 
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId.Value);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                    }
                }
            }

            // Reverse Coupon Usage
            if (!string.IsNullOrEmpty(order.DiscountCode))
            {
                var couponSpec = new CouponByCodeSpecification(order.DiscountCode);
                var coupon = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(couponSpec);
                if (coupon != null && coupon.TimesUsed > 0)
                {
                    coupon.TimesUsed--;
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            // Send cancellation email
            try
            {
                await _emailService.SendOrderCancelledEmailAsync(
                    order.Email,
                    order.CustomerName,
                    order.OrderNumber,
                    reason);
                    
                _logger.LogInformation("Cancellation email sent for order {OrderNumber}", order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation email for {OrderNumber}", order.OrderNumber);
            }
            
            return order;
        }

        public Task<Order?> CancelOrderAsync(int orderId, string reason)
        {
            return CancelOrderAsync(orderId, reason, false, 0);
        }

        public async Task<Order?> RefundOrderAsync(int orderId, decimal amount)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return null;

            if (order.Status != OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Only cancelled orders can be refunded.");
            }

            try
            {
                order.MarkAsRefunded(amount);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Order {OrderNumber} manually marked as refunded. Amount: {Amount}", order.OrderNumber, amount);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refund order {OrderNumber}", order.OrderNumber);
                throw;
            }
        }
    }
}
