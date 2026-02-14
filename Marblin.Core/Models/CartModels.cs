namespace Marblin.Core.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.Total);
        public string? AppliedCouponCode { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount => Math.Max(0, SubTotal - DiscountAmount);
    }
}
