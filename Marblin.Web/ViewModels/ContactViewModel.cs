using System.ComponentModel.DataAnnotations;

namespace Marblin.Web.ViewModels
{
    /// <summary>
    /// ViewModel for the Contact Us form.
    /// </summary>
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your message")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}
