namespace Marblin.Core.Entities
{
    /// <summary>
    /// Product category for organizing catalog items (e.g., Tables, Countertops, Decorative).
    /// </summary>
    public class Category
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Display order in navigation/listings.
        /// </summary>
        public int SortOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
