using _2tlob.Data;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Cart;
using Microsoft.EntityFrameworkCore;

namespace _2tlob.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(string customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Seller)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartViewModel> GetCartAsync(string customerId)
        {
            var cart = await GetOrCreateCartAsync(customerId);

            var items = cart.Items.Select(item => new CartItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductImageUrl = item.Product.ImageUrl,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                AvailableStock = item.Product.AvailableQuantity,
                SellerName = item.Product.Seller?.FullName ?? "Unknown seller"
            }).ToList();

            return new CartViewModel { Items = items };
        }

        public async Task<(bool Success, string? ErrorMessage)> AddToCartAsync(string customerId, int productId, int quantity)
        {
            if (quantity < 1)
            {
                return (false, "Quantity must be at least 1.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            if (product.AvailableQuantity <= 0)
            {
                return (false, "This product is currently out of stock.");
            }

            var cart = await GetOrCreateCartAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            int requestedTotal = (existingItem?.Quantity ?? 0) + quantity;

            // Server-side check: prevent adding qty > Product.AvailableQuantity
            if (requestedTotal > product.AvailableQuantity)
            {
                return (false, $"Only {product.AvailableQuantity} unit(s) of '{product.Name}' available, and you already have {existingItem?.Quantity ?? 0} in your cart.");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = requestedTotal;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        private async Task<CartItem?> GetOwnedCartItemAsync(string customerId, int cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.CustomerId == customerId);
        }

        public async Task<(bool Success, string? ErrorMessage)> IncreaseQuantityAsync(string customerId, int cartItemId)
        {
            var item = await GetOwnedCartItemAsync(customerId, cartItemId);
            if (item == null)
            {
                return (false, "Cart item not found.");
            }

            if (item.Quantity + 1 > item.Product.AvailableQuantity)
            {
                return (false, $"Only {item.Product.AvailableQuantity} unit(s) of '{item.Product.Name}' available.");
            }

            item.Quantity += 1;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DecreaseQuantityAsync(string customerId, int cartItemId)
        {
            var item = await GetOwnedCartItemAsync(customerId, cartItemId);
            if (item == null)
            {
                return (false, "Cart item not found.");
            }

            if (item.Quantity <= 1)
            {
                return (false, "Minimum quantity is 1. Remove the item if you don't want it anymore.");
            }

            item.Quantity -= 1;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveFromCartAsync(string customerId, int cartItemId)
        {
            var item = await GetOwnedCartItemAsync(customerId, cartItemId);
            if (item == null)
            {
                return (false, "Cart item not found.");
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<int> GetCartItemCountAsync(string customerId)
        {
            return await _context.CartItems
                .Where(ci => ci.Cart.CustomerId == customerId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;
        }
    }
}
