using Marblin.Core.Entities;
using Marblin.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Marblin.Application.DTOs
{
    public class OrderSubmissionDto
    {
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Region { get; set; } = string.Empty;

        [Required]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = "Egypt";
    }
}
