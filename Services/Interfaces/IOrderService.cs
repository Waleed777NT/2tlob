using _2tlob.Enum;
using _2tlob.Models;
using _2tlob.ViewModels.Checkout;
using _2tlob.ViewModels.Order;

namespace _2tlob.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, int? OrderId, string? ErrorMessage)> CheckoutAsync(string customerId, CheckoutViewModel model);
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId);
        Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string currentUserId, bool isSeller, bool isAdmin);
        Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, string currentUserId, bool isAdmin);
        Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string currentUserId, bool isAdmin, bool isSeller);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
    }
}
