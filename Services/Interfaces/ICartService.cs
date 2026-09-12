using _2tlob.ViewModels.Cart;

namespace _2tlob.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string customerId);
        Task<(bool Success, string? ErrorMessage)> AddToCartAsync(string customerId, int productId, int quantity);
        Task<(bool Success, string? ErrorMessage)> IncreaseQuantityAsync(string customerId, int cartItemId);
        Task<(bool Success, string? ErrorMessage)> DecreaseQuantityAsync(string customerId, int cartItemId);
        Task<(bool Success, string? ErrorMessage)> RemoveFromCartAsync(string customerId, int cartItemId);
        Task<int> GetCartItemCountAsync(string customerId);
    }
}
