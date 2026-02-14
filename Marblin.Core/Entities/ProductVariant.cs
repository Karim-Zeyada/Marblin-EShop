using System.ComponentModel.DataAnnotations;

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
        /// Optional sale price for this specific variant.
        /// Only active when the parent product's sale window is active.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
        public decimal? SalePrice { get; set; }
        
        /// <summary>
        /// Current stock quantity.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
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
        /// Does not account for product-level sales.
        /// </summary>
        public decimal GetFinalPrice(decimal basePrice) => basePrice + PriceAdjustment;
        
        /// <summary>
        /// Sale-aware final price. When the product is on sale:
        /// - If this variant has its own SalePrice, returns that directly.
        /// - Otherwise, uses the product's active price + PriceAdjustment.
        /// </summary>
        public decimal GetFinalPrice(decimal basePrice, bool isProductOnSale)
        {
            if (isProductOnSale && SalePrice.HasValue)
                return SalePrice.Value;
            
            return basePrice + PriceAdjustment;
        }
    }
}
