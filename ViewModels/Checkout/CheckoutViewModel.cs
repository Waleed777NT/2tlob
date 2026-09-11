using _2tlob.ViewModels.Cart;
using System.ComponentModel.DataAnnotations;

namespace _2tlob.ViewModels.Checkout
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Shipping address is required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters.")]
        [Display(Name = "Street Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters.")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Postal Code must be between 3 and 20 characters.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(30)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Order Notes (Optional)")]
        public string? Notes { get; set; }

        public CartViewModel? Cart { get; set; }
    }
}
