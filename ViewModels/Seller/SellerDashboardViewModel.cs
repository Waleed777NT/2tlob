using _2tlob.DTOs.Seller;
using _2tlob.Models;
using _2tlob.ViewModels.Order;

namespace _2tlob.ViewModels.Seller
{
    public class SellerDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalOrdersCount { get; set; }
        public decimal TotalSalesRevenue { get; set; }

        // Overall seller rating: plain average of every rating (1-5) left across all
        // of this seller's products, from 0 (no reviews yet) up to 5.
        public double SellerAverageRating { get; set; }
        public int SellerReviewCount { get; set; }

        public IEnumerable<_2tlob.Models.Product> RecentProducts { get; set; } = new List<_2tlob.Models.Product>();
        public List<SellerOrderItemDto> RecentOrders { get; set; } = new List<SellerOrderItemDto>();
    }
}
