using Marblin.Core.Entities;

namespace Marblin.Core.Specifications
{
    public class OrderWithItemsSpecification : BaseSpecification<Order>
    {
        public OrderWithItemsSpecification(string orderNumber) 
            : base(o => o.OrderNumber == orderNumber)
        {
            AddInclude(o => o.OrderItems);
        }

        public OrderWithItemsSpecification(int orderId)
            : base(o => o.Id == orderId)
        {
            AddInclude(o => o.OrderItems);
        }
    }
}
