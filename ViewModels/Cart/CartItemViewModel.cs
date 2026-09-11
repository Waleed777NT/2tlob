namespace _2tlob.ViewModels.Cart
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public string SellerName { get; set; } = string.Empty;

        public decimal LineTotal => UnitPrice * Quantity;
        public bool IsOutOfStock => AvailableStock <= 0;
        public bool HasInsufficientStock => Quantity > AvailableStock;
    }
}
