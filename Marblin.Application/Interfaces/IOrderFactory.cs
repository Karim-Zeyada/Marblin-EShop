using Marblin.Application.DTOs;
using Marblin.Core.Entities;
using Marblin.Core.Models;

namespace Marblin.Application.Interfaces
{
    public interface IOrderFactory
    {
        Order CreateOrder(OrderSubmissionDto submission, ShoppingCart cart, decimal depositPercentage);
    }
}
