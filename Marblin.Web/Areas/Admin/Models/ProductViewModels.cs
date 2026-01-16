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

        public ProductAvailability Availability { get; set; } = ProductAvailability.InStock;
    }

    public class ProductEditViewModel : ProductCreateViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<ProductVariant> Variants { get; set; } = new();
        public List<ProductImage> Images { get; set; } = new();
    }
}
