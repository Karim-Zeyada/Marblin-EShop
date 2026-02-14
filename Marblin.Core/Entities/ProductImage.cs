using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Product image for high-resolution gallery display.
    /// </summary>
    public class ProductImage
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        /// <summary>
        /// Relative path to image in wwwroot/uploads/.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
        
        public string? AltText { get; set; }
        
        /// <summary>
        /// Display order in gallery.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Sort order cannot be negative")]
        public int SortOrder { get; set; }
        
        /// <summary>
        /// Primary image shown in listings and thumbnails.
        /// </summary>
        public bool IsPrimary { get; set; }
        
        // Navigation
        public virtual Product Product { get; set; } = null!;
    }
}
