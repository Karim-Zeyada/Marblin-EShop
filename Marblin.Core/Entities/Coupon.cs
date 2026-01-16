
namespace Marblin.Core.Entities
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Discount percentage (0-100). If set, takes precedence over fixed amount? 
        /// Or we allow only one type. Let's allowing nullable for both.
        /// </summary>
        public decimal? DiscountPercentage { get; set; }
        
        /// <summary>
        /// Fixed discount amount.
        /// </summary>
        public decimal? DiscountAmount { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        public int? UsageLimit { get; set; }
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
