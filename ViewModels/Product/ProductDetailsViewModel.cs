using _2tlob.Models;

namespace _2tlob.ViewModels.Product
{
    public class ProductDetailsViewModel
    {
        public _2tlob.Models.Product Product { get; set; } = null!;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public IEnumerable<_2tlob.Models.Review> Reviews { get; set; } = new List<_2tlob.Models.Review>();
        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();

        public bool CanReview { get; set; }
        public bool HasReviewed { get; set; }
        public bool IsInWishlist { get; set; }
        public bool IsOwner { get; set; }
    }
}
