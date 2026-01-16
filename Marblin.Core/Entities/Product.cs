using Marblin.Core.Enums;

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
        
        // Navigation
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
