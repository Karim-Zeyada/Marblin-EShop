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
        
        /// <summary>
        /// Value statements/descriptions (JSON array or pipe-separated).
        /// </summary>
        public string? ValueStatements { get; set; }
        
        /// <summary>
        /// Process steps text (JSON array or pipe-separated).
        /// </summary>
        public string? ProcessSteps { get; set; }
        
        // Business Contact Info
        public string? InstapayAccount { get; set; }
        public string? VodafoneCashNumber { get; set; }
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
