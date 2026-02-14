using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Custom product request (quote) submitted by customers for bespoke items.
    /// </summary>
    public class CustomRequest
    {
        public int Id { get; set; }
        
        // Customer Information
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        // Request Details
        /// <summary>
        /// Product category (e.g., Table, Countertop, Furniture, Other).
        /// </summary>
        [Required(ErrorMessage = "Category is required")]
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
        [Required(ErrorMessage = "Please describe your request")]
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
        
        // Admin Tracking
        public bool IsReviewed { get; set; }
        public string? AdminNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public virtual ICollection<CustomRequestImage> Images { get; set; } = new List<CustomRequestImage>();
    }
}
