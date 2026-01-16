namespace Marblin.Core.Entities
{
    /// <summary>
    /// Individual item in an order, capturing product/variant snapshot at purchase time.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        /// <summary>
        /// Reference to original product (nullable if product deleted).
        /// </summary>
        public int? ProductId { get; set; }
        
        /// <summary>
        /// Reference to original variant (nullable if variant deleted).
        /// </summary>
        public int? VariantId { get; set; }
        
        // Snapshot of product info at time of purchase (for historical accuracy)
        public string ProductName { get; set; } = string.Empty;
        public string? VariantDescription { get; set; }
        public string? ImageUrl { get; set; }
        
        public int Quantity { get; set; }
        
        /// <summary>
        /// Unit price at time of purchase.
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Line total (Quantity * UnitPrice).
        /// </summary>
        public decimal LineTotal => Quantity * UnitPrice;
        
        // Navigation
        public virtual Order Order { get; set; } = null!;
        public virtual Product? Product { get; set; }
        public virtual ProductVariant? Variant { get; set; }
    }
}
