namespace Marblin.Core.Entities
{
    /// <summary>
    /// Custom product request (quote) submitted by customers for bespoke items.
    /// </summary>
    public class CustomRequest
    {
        public int Id { get; set; }
        
        // Customer Information
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        // Request Details
        /// <summary>
        /// Product category (e.g., Table, Countertop, Furniture, Other).
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Desired dimensions (free text).
        /// </summary>
        public string? Dimensions { get; set; }
        
        /// <summary>
        /// Preferred material (e.g., Black Marble, White Marble, Travertine, Not Sure).
        /// </summary>
        public string? Material { get; set; }
        
        /// <summary>
        /// Budget range (optional, free text).
        /// </summary>
        public string? BudgetRange { get; set; }
        
        /// <summary>
        /// Project timeline (optional, free text).
        /// </summary>
        public string? Timeline { get; set; }
        
        /// <summary>
        /// Additional notes/description (required).
        /// </summary>
        public string Notes { get; set; } = string.Empty;
        
        // Admin Tracking
        public bool IsReviewed { get; set; }
        public string? AdminNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public virtual ICollection<CustomRequestImage> Images { get; set; } = new List<CustomRequestImage>();
    }
}
