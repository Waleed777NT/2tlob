using _2tlob.Enum;
using _2tlob.Models;

namespace _2tlob.ViewModels.Order
{
    public class OrderDetailsViewModel
    {
        public _2tlob.Models.Order Order { get; set; } = null!;
        public IEnumerable<OrderItem> Items { get; set; } = new List<OrderItem>();
        public bool CanCancel => Order.Status == OrderStatus.Pending || Order.Status == OrderStatus.Confirmed;
        public bool IsSellerView { get; set; }
        public bool IsAdminView { get; set; }
        public decimal SellerSubtotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
