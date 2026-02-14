
using System.ComponentModel.DataAnnotations;

namespace Marblin.Core.Entities
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Discount percentage (0-100). If set, takes precedence over fixed amount? 
        /// Or we allow only one type. Let's allowing nullable for both.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal? DiscountPercentage { get; set; }
        
        /// <summary>
        /// Fixed discount amount.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Discount amount cannot be negative")]
        public decimal? DiscountAmount { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be at least 1")]
        public int? UsageLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Times used cannot be negative")]
        public int TimesUsed { get; set; }

        public bool IsValid()
        {
            if (!IsActive) return false;
            if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow) return false;
            if (UsageLimit.HasValue && TimesUsed >= UsageLimit.Value) return false;
            return true;
        }
    }
}
