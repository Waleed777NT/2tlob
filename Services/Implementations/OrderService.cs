using _2tlob.Data;
using _2tlob.Enum;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Checkout;
using _2tlob.ViewModels.Order;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace _2tlob.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, int? OrderId, string? ErrorMessage)> CheckoutAsync(string customerId, CheckoutViewModel model)
        {
            // Begin atomic database transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || !cart.Items.Any())
                {
                    return (false, null, "Your shopping cart is empty.");
                }

                // Verify and deduct stock atomically
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in cart.Items)
                {
                    // Reload fresh product record from database to prevent stale reads
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return (false, null, $"Product '{item.Product.Name}' is no longer available.");
                    }

                    if (product.AvailableQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return (false, null, $"Insufficient stock for '{product.Name}'. Only {product.AvailableQuantity} available.");
                    }

                    // Decrement stock
                    product.AvailableQuantity -= item.Quantity;

                    // Calculate line total using fresh DB price
                    decimal linePrice = product.Price * item.Quantity;
                    totalAmount += linePrice;

                    // Snapshot unit price and seller ID at checkout time
                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        SellerId = product.SellerId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    ShippingAddress = model.ShippingAddress.Trim(),
                    City = model.City.Trim(),
                    PostalCode = model.PostalCode.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Notes = model.Notes?.Trim(),
                    Items = orderItems
                };

                _context.Orders.Add(order);

                // Clear customer's shopping cart
                _context.CartItems.RemoveRange(cart.Items);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, order.Id, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, null, $"An error occurred during checkout: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string currentUserId, bool isSeller, bool isAdmin)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Seller)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            // Customer View: MUST match customer ID
            if (!isAdmin && !isSeller)
            {
                if (order.CustomerId != currentUserId)
                {
                    return null; // Forbidden / Not Found for other customers
                }

                return new OrderDetailsViewModel
                {
                    Order = order,
                    Items = order.Items.ToList(),
                    IsSellerView = false,
                    IsAdminView = false
                };
            }

            // Seller View: Filter to only show items belonging to this seller
            if (isSeller && !isAdmin)
            {
                var sellerItems = order.Items.Where(i => i.SellerId == currentUserId).ToList();
                if (!sellerItems.Any())
                {
                    return null; // Seller has no items in this order
                }

                return new OrderDetailsViewModel
                {
                    Order = order,
                    Items = sellerItems,
                    IsSellerView = true,
                    IsAdminView = false
                };
            }

            // Admin View: Full access
            return new OrderDetailsViewModel
            {
                Order = order,
                Items = order.Items.ToList(),
                IsSellerView = false,
                IsAdminView = true
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, string currentUserId, bool isAdmin)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return (false, "Order not found.");
                }

                // Ownership check for non-admin
                if (!isAdmin && order.CustomerId != currentUserId)
                {
                    return (false, "Unauthorized: You do not own this order.");
                }

                // Can only cancel while the order is still Pending or Confirmed (not yet shipped)
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                {
                    return (false, $"Cannot cancel order with status '{order.Status}'. Only 'Pending' or 'Confirmed' orders can be cancelled.");
                }

                // Restore stock for all items
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.AvailableQuantity += item.Quantity;
                    }
                }

                order.Status = OrderStatus.Cancelled;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to cancel order: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string currentUserId, bool isAdmin, bool isSeller)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return (false, "Order not found.");
            }

            // If seller, verify seller has products in this order
            if (isSeller && !isAdmin)
            {
                bool ownsItemsInOrder = order.Items.Any(i => i.SellerId == currentUserId);
                if (!ownsItemsInOrder)
                {
                    return (false, "Unauthorized: You do not have products in this order.");
                }
            }

            // Validate status transitions state machine
            var currentStatus = order.Status;

            if (currentStatus == OrderStatus.Cancelled)
            {
                return (false, "Cannot modify a cancelled order.");
            }

            if (currentStatus == OrderStatus.Delivered && newStatus != OrderStatus.Delivered)
            {
                return (false, "Cannot change status of a delivered order.");
            }

            bool isValidTransition = (currentStatus, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Shipped, OrderStatus.Delivered) => true,
                _ => false
            };

            if (!isValidTransition)
            {
                return (false, $"Invalid status transition from '{currentStatus}' to '{newStatus}'.");
            }

            // If cancelling via status update, restore inventory
            if (newStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.AvailableQuantity += item.Quantity;
                    }
                }
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
