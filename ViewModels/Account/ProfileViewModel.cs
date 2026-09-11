using _2tlob.Enum;
using _2tlob.Models;
using System.ComponentModel.DataAnnotations;

namespace _2tlob.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = "Customer";
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedAt { get; set; }

        public SellerRequest? PendingSellerRequest { get; set; }
        public int TotalOrders { get; set; }
        public int WishlistCount { get; set; }
    }
}
