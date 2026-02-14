using Marblin.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Core product entity representing marble/stone artifacts.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        /// <summary>
        /// Base price before variant adjustments.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }
        
        public int CategoryId { get; set; }
        
        /// <summary>
        /// Featured on home page Signature Pieces section.
        /// </summary>
        public bool IsSignaturePiece { get; set; }
        
        public ProductAvailability Availability { get; set; } = ProductAvailability.InStock;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Sale Pricing
        /// <summary>
        /// Discounted price during sale period. Null means no sale.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
        public decimal? SalePrice { get; set; }
        
        /// <summary>
        /// When the sale becomes active.
        /// </summary>
        public DateTime? SaleStartDate { get; set; }
        
        /// <summary>
        /// When the sale ends.
        /// </summary>
        public DateTime? SaleEndDate { get; set; }
        
        /// <summary>
        /// Featured in special "On Sale" promotions.
        /// </summary>
        public bool IsFeaturedSale { get; set; }
        
        // Navigation
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        
        /// <summary>
        /// Check if product is currently on sale.
        /// </summary>
        public bool IsOnSale()
        {
            return SalePrice.HasValue 
                && SalePrice < BasePrice
                && (!SaleStartDate.HasValue || SaleStartDate <= DateTime.UtcNow)
                && (!SaleEndDate.HasValue || SaleEndDate > DateTime.UtcNow);
        }
        
        /// <summary>
        /// Get the current active price (sale price if on sale, otherwise base price).
        /// </summary>
        public decimal GetActivePrice() => IsOnSale() ? SalePrice!.Value : BasePrice;
        
        /// <summary>
        /// Get the discount percentage if on sale.
        /// </summary>
        public int GetDiscountPercentage()
        {
            if (!IsOnSale() || BasePrice == 0) return 0;
            return (int)Math.Round((1 - (SalePrice!.Value / BasePrice)) * 100);
        }
    }
}
