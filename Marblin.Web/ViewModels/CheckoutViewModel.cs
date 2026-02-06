using System.ComponentModel.DataAnnotations;
using Marblin.Core.Entities;
using Marblin.Core.Models;

namespace Marblin.Web.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Street Address")]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Region { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = "Egypt";
        
        public ShoppingCart? Cart { get; set; }
        public SiteSettings? Settings { get; set; }
    }
}
