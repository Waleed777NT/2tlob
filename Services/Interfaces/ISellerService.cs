using _2tlob.Models;
using _2tlob.ViewModels.Order;
using _2tlob.ViewModels.Seller;

namespace _2tlob.Services.Interfaces
{
    public interface ISellerService
    {
        Task<(bool Success, string? ErrorMessage)> SubmitRequestAsync(string userId, SellerRequestCreateViewModel model);
        Task<bool> HasPendingRequestAsync(string userId);
        Task<SellerRequest?> GetUserPendingRequestAsync(string userId);
        Task<IEnumerable<SellerRequest>> GetPendingRequestsAsync();
        Task<IEnumerable<SellerRequest>> GetAllRequestsAsync();
        Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int requestId);
        Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId, string? adminNotes);
        Task<SellerDashboardViewModel> GetSellerDashboardAsync(string sellerId);
        Task<SellerOrderListViewModel> GetSellerOrdersAsync(string sellerId);
    }
}
