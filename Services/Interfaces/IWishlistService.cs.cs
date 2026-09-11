using _2tlob.ViewModels.Wishlist;

namespace _2tlob.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<WishlistViewModel> GetWishlistByCustomerIdAsync(string customerId);
        Task<(bool Success, string? ErrorMessage)> AddToWishlistAsync(string customerId, int productId);
        Task<(bool Success, string? ErrorMessage)> RemoveFromWishlistAsync(string customerId, int productId);
        Task<bool> IsInWishlistAsync(string customerId, int productId);
        Task<int> GetWishlistItemCountAsync(string customerId);
        Task<HashSet<int>> GetWishlistProductIdsAsync(string customerId);
    }
}
