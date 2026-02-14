using Marblin.Application.DTOs;
using Marblin.Application.Interfaces;
using Marblin.Core.Entities;
using Marblin.Core.Models;

namespace Marblin.Application.Services
{
    public class OrderFactory : IOrderFactory
    {
        public Order CreateOrder(OrderSubmissionDto model, ShoppingCart cart, decimal depositPercentage)
        {
            var depositAmount = (cart.TotalAmount * depositPercentage) / 100m;
            var orderNumber = $"M-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerName = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                AddressLine = model.AddressLine,
                City = model.City,
                Region = model.Region,
                Country = model.Country,
                TotalAmount = cart.TotalAmount,
                DiscountCode = cart.AppliedCouponCode,
                DiscountAmount = cart.DiscountAmount,
                DepositPercentage = depositPercentage,
                DepositAmount = depositAmount,
                CreatedAt = DateTime.UtcNow,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            return order;
        }
    }
}
