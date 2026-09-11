using _2tlob.Data;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Wishlist;
using Microsoft.EntityFrameworkCore;

namespace _2tlob.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;

        public WishlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Wishlist> GetOrCreateWishlistAsync(string customerId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(w => w.CustomerId == customerId);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return wishlist;
        }

        public async Task<WishlistViewModel> GetWishlistByCustomerIdAsync(string customerId)
        {
            var wishlist = await GetOrCreateWishlistAsync(customerId);

            var items = wishlist.Items.Select(item => new WishlistItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductImageUrl = item.Product.ImageUrl,
                Price = item.Product.Price,
                AvailableQuantity = item.Product.AvailableQuantity,
                CategoryName = item.Product.Category?.Name ?? "General",
                AddedAt = item.AddedAt
            }).OrderByDescending(i => i.AddedAt).ToList();

            return new WishlistViewModel
            {
                Items = items
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> AddToWishlistAsync(string customerId, int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            var wishlist = await GetOrCreateWishlistAsync(customerId);

            bool alreadyExists = wishlist.Items.Any(i => i.ProductId == productId);
            if (alreadyExists)
            {
                return (false, "This product is already in your wishlist.");
            }

            var item = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            };

            _context.WishlistItems.Add(item);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveFromWishlistAsync(string customerId, int productId)
        {
            var wishlist = await GetOrCreateWishlistAsync(customerId);
            var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                return (false, "Item not found in your wishlist.");
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> IsInWishlistAsync(string customerId, int productId)
        {
            return await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .AnyAsync(wi => wi.Wishlist.CustomerId == customerId && wi.ProductId == productId);
        }

        public async Task<int> GetWishlistItemCountAsync(string customerId)
        {
            return await _context.WishlistItems
                .Where(wi => wi.Wishlist.CustomerId == customerId)
                .CountAsync();
        }

        public async Task<HashSet<int>> GetWishlistProductIdsAsync(string customerId)
        {
            var ids = await _context.WishlistItems
                .Where(wi => wi.Wishlist.CustomerId == customerId)
                .Select(wi => wi.ProductId)
                .ToListAsync();

            return ids.ToHashSet();
        }
    }
}
