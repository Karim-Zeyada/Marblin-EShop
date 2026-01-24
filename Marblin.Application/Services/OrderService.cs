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
                
                // [FLOW 1] Send Confirmation Email
                try
                {
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
                        proofUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send order confirmation email for {OrderNumber}", order.OrderNumber);
                }

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
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order != null)
            {
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
                            order.DepositAmount, // Assuming deposit is verified if moving to this stage
                            order.RemainingBalance,
                            settings?.InstapayAccount ?? "N/A",
                            settings?.VodafoneCashNumber ?? "N/A");
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
        }

        public async Task VerifyDepositAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order != null)
            {
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
                        order.RemainingBalance);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send deposit verification email for {OrderNumber}", order.OrderNumber);
                }
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
                await _emailService.SendPaymentProofReceivedEmailAsync(order.Email, order.CustomerName, order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment proof received email for {OrderNumber}", order.OrderNumber);
            }
        }

        public async Task VerifyBalanceAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order != null)
            {
                order.VerifyBalance();
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Balance verified for Order {OrderId}", orderId);

                // No specific flow requested for Balance verify yet, but could move to Processing/Shipped.
                // Shipped is a separate manual update in user requirement Flow 5.
            }
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
    }
}
