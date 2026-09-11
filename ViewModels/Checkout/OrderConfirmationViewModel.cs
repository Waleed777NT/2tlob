using _2tlob.Enum;
using _2tlob.Models;

namespace _2tlob.ViewModels.Checkout
{
    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
