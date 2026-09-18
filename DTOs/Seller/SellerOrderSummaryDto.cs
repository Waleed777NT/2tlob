using _2tlob.Enum;

namespace _2tlob.DTOs.Seller
{
    // One row per Order (grouping this seller's items within it together),
    // used for the Seller "My Orders" list page.
    public class SellerOrderSummaryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
