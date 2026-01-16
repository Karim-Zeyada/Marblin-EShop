namespace Marblin.Core.Entities
{
    /// <summary>
    /// Product variant representing material/size combinations with price adjustments.
    /// </summary>
    public class ProductVariant
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        /// <summary>
        /// Material type (e.g., Black Marble, White Marble, Travertine).
        /// </summary>
        public string Material { get; set; } = string.Empty;
        
        /// <summary>
        /// Size specification (e.g., "60x120cm", "Large").
        /// </summary>
        public string Size { get; set; } = string.Empty;
        
        /// <summary>
        /// Price adjustment from base price (can be positive or negative).
        /// </summary>
        public decimal PriceAdjustment { get; set; }
        
        /// <summary>
        /// Current stock quantity.
        /// </summary>
        public int Stock { get; set; }
        
        /// <summary>
        /// Stock Keeping Unit identifier.
        /// </summary>
        public string? SKU { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation
        public virtual Product Product { get; set; } = null!;
        
        /// <summary>
        /// Calculated final price (BasePrice + PriceAdjustment).
        /// </summary>
        public decimal GetFinalPrice(decimal basePrice) => basePrice + PriceAdjustment;
    }
}
