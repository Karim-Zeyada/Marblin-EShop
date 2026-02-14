using Marblin.Core.Entities;
using Marblin.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Marblin.Web.Areas.Admin.Models
{
    public class ProductCreateViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Base Price")]
        public decimal BasePrice { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Signature Piece")]
        public bool IsSignaturePiece { get; set; }

        [Required]
        [Display(Name = "Stock")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Display(Name = "SKU")]
        [StringLength(50)]
        public string? SKU { get; set; }

        public ProductAvailability Availability { get; set; } = ProductAvailability.InStock;
    }

    public class ProductEditViewModel : ProductCreateViewModel, IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        // Sale Pricing
        [Display(Name = "Sale Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
        public decimal? SalePrice { get; set; }
        
        [Display(Name = "Sale Start Date")]
        public DateTime? SaleStartDate { get; set; }
        
        [Display(Name = "Sale End Date")]
        public DateTime? SaleEndDate { get; set; }
        
        [Display(Name = "Featured Sale")]
        public bool IsFeaturedSale { get; set; }

        public List<ProductImage> Images { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SalePrice.HasValue && SalePrice >= BasePrice)
            {
                yield return new ValidationResult(
                    "Sale price must be less than the base price.",
                    new[] { nameof(SalePrice) });
            }

            if (SaleStartDate.HasValue && SaleEndDate.HasValue && SaleEndDate <= SaleStartDate)
            {
                yield return new ValidationResult(
                    "Sale end date must be after the start date.",
                    new[] { nameof(SaleEndDate) });
            }
        }
    }
}
