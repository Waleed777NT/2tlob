using _2tlob.Models;

namespace _2tlob.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PendingSellerRequests { get; set; }
        public decimal TotalRevenue { get; set; }

        public IEnumerable<_2tlob.Models.Order> RecentOrders { get; set; } = new List<_2tlob.Models.Order>();
        public IEnumerable<SellerRequest> RecentSellerRequests { get; set; } = new List<SellerRequest>();
    }
}
