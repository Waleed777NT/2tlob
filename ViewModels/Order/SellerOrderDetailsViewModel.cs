using _2tlob.DTOs.Seller;
using _2tlob.Enum;

namespace _2tlob.ViewModels.Order
{
    // The single-order breakdown page: this seller's items within one specific order.
    public class SellerOrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public OrderStatus OrderStatus { get; set; }
        public List<SellerOrderItemDto> Items { get; set; } = new List<SellerOrderItemDto>();
        public decimal TotalAmount => Items.Sum(i => i.LineTotal);
        public bool CanUpdateStatus => OrderStatus != OrderStatus.Delivered && OrderStatus != OrderStatus.Cancelled;
    }
}
