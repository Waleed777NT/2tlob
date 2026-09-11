using _2tlob.Models;
using System.ComponentModel.DataAnnotations;

namespace _2tlob.ViewModels.Seller
{
    public class SellerRequestCreateViewModel
    {
        [Required(ErrorMessage = "Business name is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Business Name must be between 3 and 150 characters.")]
        [Display(Name = "Business / Store Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe what products you intend to sell and your business background.")]
        [StringLength(1000, MinimumLength = 20, ErrorMessage = "Reason must be at least 20 characters.")]
        [Display(Name = "Business Description / Reason for Application")]
        public string Reason { get; set; } = string.Empty;

        [Phone]
        [StringLength(50)]
        [Display(Name = "Contact Phone Number (Optional)")]
        public string? PhoneNumber { get; set; }
    }

    public class SellerRequestListViewModel
    {
        public IEnumerable<SellerRequest> Requests { get; set; } = new List<SellerRequest>();
    }
}
