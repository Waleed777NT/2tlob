using System.ComponentModel.DataAnnotations;

namespace _2tlob.ViewModels.Review
{
    public class AddReviewViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;

        // True when the customer already has a review for this product and is
        // editing it, instead of writing a brand new one.
        public bool IsEditing { get; set; }

        [Required(ErrorMessage = "Please select a star rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Please write a comment.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 1000 characters.")]
        [Display(Name = "Your Review")]
        public string Comment { get; set; } = string.Empty;
    }
}

