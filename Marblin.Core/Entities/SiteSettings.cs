using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Global site settings for deposit configuration and CMS content.
    /// Only one record should exist in the database.
    /// </summary>
    public class SiteSettings
    {
        public int Id { get; set; }
        
        // Deposit Configuration
        /// <summary>
        /// Global deposit percentage for new orders (e.g., 10, 20, 50).
        /// </summary>
        [Range(1, 100, ErrorMessage = "Deposit percentage must be between 1 and 100")]
        public decimal DepositPercentage { get; set; } = 10m;
        
        // Home Page CMS Content
        public string? HeroHeadline { get; set; }
        public string? HeroSubheadline { get; set; }
        public string? HeroImageUrl { get; set; }
        
        // Feature Section (Custom Work / Statement)
        public string? FeatureHeadline { get; set; }
        public string? FeatureBody { get; set; }
        public string? FeatureImageUrl { get; set; }
        public string? FeatureButtonText { get; set; }
        public string? FeatureButtonUrl { get; set; }
        

        // Business Contact Info
        public string? InstapayAccount { get; set; }
        public string? VodafoneCashNumber { get; set; }
        
        // Shipping Configuration
        /// <summary>
        /// Fixed shipping cost for Cairo and Giza deliveries.
        /// Shipping for other cities will be calculated separately.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Shipping cost cannot be negative")]
        public decimal CairoGizaShippingCost { get; set; } = 0m;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
