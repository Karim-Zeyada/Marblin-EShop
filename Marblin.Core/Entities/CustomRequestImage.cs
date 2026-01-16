namespace Marblin.Core.Entities
{
    /// <summary>
    /// Inspiration image attached to a custom request.
    /// </summary>
    public class CustomRequestImage
    {
        public int Id { get; set; }
        
        public int CustomRequestId { get; set; }
        
        /// <summary>
        /// Relative path to image in wwwroot/uploads/.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public virtual CustomRequest CustomRequest { get; set; } = null!;
    }
}
