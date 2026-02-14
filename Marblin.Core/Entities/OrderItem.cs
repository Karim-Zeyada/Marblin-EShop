using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Individual item in an order, capturing product snapshot at purchase time.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        /// <summary>
        /// Reference to original product (nullable if product deleted).
        /// </summary>
        public int? ProductId { get; set; }
        
        // Snapshot of product info at time of purchase (for historical accuracy)
        public string ProductName { get; set; } = string.Empty;
        public string? VariantDescription { get; set; }
        public string? ImageUrl { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        /// <summary>
        /// Unit price at time of purchase.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Line total (Quantity * UnitPrice).
        /// </summary>
        public decimal LineTotal => Quantity * UnitPrice;
        
        // Navigation
        public virtual Order Order { get; set; } = null!;
        public virtual Product? Product { get; set; }
    }
}
