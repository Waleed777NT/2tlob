using _2tlob.Models;

namespace _2tlob.ViewModels.Product
{
    public class ProductListViewModel
    {
        public IEnumerable<_2tlob.Models.Product> Products { get; set; } = new List<_2tlob.Models.Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "newest", "name_asc"

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public HashSet<int> WishlistProductIds { get; set; } = new HashSet<int>();
    }
}
