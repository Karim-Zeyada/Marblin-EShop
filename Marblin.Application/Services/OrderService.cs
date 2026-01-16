using Marblin.Application.Interfaces;
using Marblin.Application.DTOs;
using Marblin.Core.Entities;
using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Marblin.Core.Models;
using Microsoft.Extensions.Logging;
using Marblin.Core.Specifications;

namespace Marblin.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IOrderFactory _orderFactory;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork, 
            IFileService fileService, 
            IOrderFactory orderFactory,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _orderFactory = orderFactory;
            _logger = logger;
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
                
                _logger.LogInformation("Order {OrderNumber} created successfully. Deposit: {DepositAmount}", 
                    order.OrderNumber, order.DepositAmount);
                
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
                order.UpdateStatus(newStatus);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Order {OrderId} status changed to {Status}", orderId, newStatus);
            }
        }

        public async Task VerifyDepositAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order != null)
            {
                order.VerifyDeposit();
                await _unitOfWork.SaveChangesAsync();
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
            if (order == null)
            {
                _logger.LogWarning("Attempted to upload proof for non-existent Order {OrderId}", orderId);
                return null;
            }

            var relativePath = await _fileService.SaveFileAsync(fileStream, fileName, FileCategory.ReceiptImage);
            order.SetPaymentProof(relativePath, PaymentProofType.ReceiptImage);
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Payment proof uploaded for Order {OrderId}. Type: {ProofType}", 
                orderId, PaymentProofType.ReceiptImage);
            
            return order;
        }

        public async Task<Order?> SubmitPaymentProofAsync(int orderId, string transactionId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Attempted to submit transaction ID for non-existent Order {OrderId}", orderId);
                return null;
            }

            order.SetPaymentProof(transactionId, PaymentProofType.TransactionId);
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Payment proof uploaded for Order {OrderId}. Type: {ProofType}", 
                orderId, PaymentProofType.TransactionId);
            
            return order;
        }

        public async Task<Order?> SubmitBalancePaymentProofAsync(int orderId, Stream fileStream, string fileName)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Attempted to upload balance proof for non-existent Order {OrderId}", orderId);
                return null;
            }

            var relativePath = await _fileService.SaveFileAsync(fileStream, fileName, FileCategory.ReceiptImage);
            order.SetBalancePaymentProof(relativePath, PaymentProofType.ReceiptImage);
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Balance payment proof uploaded for Order {OrderId}. Type: {ProofType}", 
                orderId, PaymentProofType.ReceiptImage);
            
            return order;
        }

        public async Task<Order?> SubmitBalancePaymentProofAsync(int orderId, string transactionId)
        {
             var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
             if (order == null)
             {
                 _logger.LogWarning("Attempted to submit balance transaction ID for non-existent Order {OrderId}", orderId);
                 return null;
             }

             order.SetBalancePaymentProof(transactionId, PaymentProofType.TransactionId);
             
             await _unitOfWork.SaveChangesAsync();
             
             _logger.LogInformation("Balance payment proof uploaded for Order {OrderId}. Type: {ProofType}", 
                 orderId, PaymentProofType.TransactionId);
             
             return order;
        }

        public async Task VerifyBalanceAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order != null)
            {
                order.VerifyBalance();
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Balance verified for Order {OrderId}", orderId);
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
