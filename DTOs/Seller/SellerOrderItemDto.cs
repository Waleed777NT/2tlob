using _2tlob.Enum;

namespace _2tlob.DTOs.Seller
{
    public class SellerOrderItemDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public OrderStatus OrderStatus { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // The rating THIS customer gave THIS product (null if they haven't reviewed it yet).
        public int? CustomerRating { get; set; }
    }

}
