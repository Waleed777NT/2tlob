namespace _2tlob.ViewModels.Wishlist
{
    public class WishlistItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
        public bool InStock => AvailableQuantity > 0;
    }

    public class WishlistViewModel
    {
        public List<WishlistItemViewModel> Items { get; set; } = new List<WishlistItemViewModel>();
        public int TotalItems => Items.Count;
    }
}
