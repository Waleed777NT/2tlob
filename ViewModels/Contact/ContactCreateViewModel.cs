using System.ComponentModel.DataAnnotations;
using _2tlob.Enum;
using _2tlob.Models;

namespace _2tlob.ViewModels.Contact
{
    public class ContactCreateViewModel
    {
        [Required(ErrorMessage = "Your name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a reason for contacting us.")]
        [Display(Name = "Reason")]
        public ContactReason Reason { get; set; } = ContactReason.General;

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Subject must be between 3 and 150 characters.")]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your message.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        // Drives the informational "your account was suspended" banner on the form.
        public bool IsSuspensionContext { get; set; }
    }
}
