using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Promotional announcement for displaying banners on customer-facing pages.
    /// </summary>
    public class Announcement
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Short title for the announcement (e.g., "Flash Sale", "Free Shipping").
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Full announcement message displayed to customers.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Visual style: "info", "sale", or "warning".
        /// </summary>
        public string Style { get; set; } = "info";
        
        /// <summary>
        /// When the announcement becomes visible.
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the announcement is no longer visible.
        /// </summary>
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(7);
        
        /// <summary>
        /// Master toggle for the announcement.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Display priority (lower numbers appear first).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Priority cannot be negative")]
        public int Priority { get; set; } = 0;
        
        // Page visibility toggles
        public bool ShowOnHomepage { get; set; } = true;
        public bool ShowOnCatalog { get; set; } = false;
        public bool ShowOnCheckout { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Check if announcement should be displayed right now.
        /// </summary>
        public bool IsCurrentlyActive()
        {
            return IsActive 
                && StartDate <= DateTime.UtcNow 
                && EndDate > DateTime.UtcNow;
        }
    }
}
