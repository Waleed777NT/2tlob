namespace _2tlob.ViewModels.Cart
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
        public decimal ShippingFee => Items.Any() ? 0.00m : 0.00m; // Free shipping
        public decimal Total => SubTotal + ShippingFee;
        public int TotalItemCount => Items.Sum(i => i.Quantity);
        public bool HasInvalidItems => Items.Any(i => i.IsOutOfStock || i.HasInsufficientStock);
    }
}
