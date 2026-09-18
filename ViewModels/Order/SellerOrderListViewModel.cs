using _2tlob.DTOs.Seller;
using _2tlob.Enum;

namespace _2tlob.ViewModels.Order
{
    // One row per Order (this seller's items in it grouped together).
    // Use SellerOrderDetailsViewModel for the per-order breakdown page.
    public class SellerOrderListViewModel
    {
        public List<SellerOrderSummaryDto> Orders { get; set; } = new List<SellerOrderSummaryDto>();
        public decimal TotalSellerSales => Orders.Where(o => o.OrderStatus != OrderStatus.Cancelled).Sum(o => o.TotalAmount);
    }
}
