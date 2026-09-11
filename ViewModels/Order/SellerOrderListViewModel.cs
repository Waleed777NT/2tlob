using _2tlob.DTOs.Seller;
using _2tlob.Enum;

namespace _2tlob.ViewModels.Order
{
    public class SellerOrderListViewModel
    {
        public List<SellerOrderItemDto> Items { get; set; } = new List<SellerOrderItemDto>();
        public decimal TotalSellerSales => Items.Where(i => i.OrderStatus != OrderStatus.Cancelled).Sum(i => i.LineTotal);
    }
}
