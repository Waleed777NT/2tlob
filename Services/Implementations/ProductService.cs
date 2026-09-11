using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Product;
using _2tlob.Enum;

namespace _2tlob.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public ProductService(ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<ProductListViewModel> GetFilteredProductsAsync(string? search, int? categoryId, string? sortBy, int page = 1, int pageSize = 12)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => (p.Name != null && p.Name.ToLower().Contains(term)) || 
                                         (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt) // newest first by default
            };

            int totalItems = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            return new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                CategoryId = categoryId,
                SortBy = sortBy,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<ProductDetailsViewModel?> GetProductDetailsAsync(int id, string? currentUserId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return null;
            }

            var reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList();
            double avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var ratingCounts = new Dictionary<int, int>
            {
                { 5, reviews.Count(r => r.Rating == 5) },
                { 4, reviews.Count(r => r.Rating == 4) },
                { 3, reviews.Count(r => r.Rating == 3) },
                { 2, reviews.Count(r => r.Rating == 2) },
                { 1, reviews.Count(r => r.Rating == 1) }
            };

            bool canReview = false;
            bool hasReviewed = false;
            bool inWishlist = false;
            bool isOwner = false;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                isOwner = product.SellerId == currentUserId;
                hasReviewed = reviews.Any(r => r.CustomerId == currentUserId);

                // User can review only if they purchased this product in a completed/confirmed/delivered order and haven't reviewed yet
                if (!hasReviewed && !isOwner)
                {
                    canReview = await _context.OrderItems
                        .Include(oi => oi.Order)
                        .AnyAsync(oi => oi.ProductId == id && 
                                       oi.Order.CustomerId == currentUserId && 
                                       oi.Order.Status != OrderStatus.Cancelled);
                }

                inWishlist = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .AnyAsync(wi => wi.ProductId == id && wi.Wishlist.CustomerId == currentUserId);
            }

            return new ProductDetailsViewModel
            {
                Product = product,
                AverageRating = Math.Round(avgRating, 1),
                ReviewCount = reviews.Count,
                Reviews = reviews,
                RatingCounts = ratingCounts,
                CanReview = canReview,
                HasReviewed = hasReviewed,
                IsInWishlist = inWishlist,
                IsOwner = isOwner
            };
        }

        public async Task<IEnumerable<Product>> GetProductsBySellerAsync(string sellerId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetProductForEditAsync(int id, string sellerId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.SellerId != sellerId)
            {
                return null;
            }

            return product;
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateProductAsync(ProductCreateViewModel model, string sellerId)
        {
            // Verify Category exists
            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
            if (!categoryExists)
            {
                return (false, "The selected category does not exist.");
            }

            string imageUrl = "/images/products/default-product.svg";

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadResult = await _fileUploadService.UploadProductImageAsync(model.ImageFile);
                if (!uploadResult.Success)
                {
                    return (false, uploadResult.ErrorMessage);
                }
                imageUrl = uploadResult.FilePath!;
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                AvailableQuantity = model.AvailableQuantity,
                CategoryId = model.CategoryId,
                SellerId = sellerId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(ProductEditViewModel model, string sellerId)
        {
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            // CRITICAL RESOURCE OWNERSHIP CHECK
            if (product.SellerId != sellerId)
            {
                return (false, "Unauthorized: You do not own this product.");
            }

            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
            if (!categoryExists)
            {
                return (false, "The selected category does not exist.");
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadResult = await _fileUploadService.UploadProductImageAsync(model.ImageFile);
                if (!uploadResult.Success)
                {
                    return (false, uploadResult.ErrorMessage);
                }

                // Delete old image if not default
                _fileUploadService.DeleteFile(product.ImageUrl);
                product.ImageUrl = uploadResult.FilePath!;
            }

            product.Name = model.Name.Trim();
            product.Description = model.Description.Trim();
            product.Price = model.Price;
            product.AvailableQuantity = model.AvailableQuantity;
            product.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateStockAsync(int productId, string sellerId, int newQuantity)
        {
            if (newQuantity < 0)
            {
                return (false, "Stock quantity cannot be negative.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            // CRITICAL RESOURCE OWNERSHIP CHECK
            if (product.SellerId != sellerId)
            {
                return (false, "Unauthorized: You do not own this product.");
            }

            product.AvailableQuantity = newQuantity;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(int productId, string sellerId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            // CRITICAL RESOURCE OWNERSHIP CHECK
            if (product.SellerId != sellerId)
            {
                return (false, "Unauthorized: You do not own this product.");
            }

            // Check if product is in any active (non-completed/non-cancelled) orders
            bool inActiveOrders = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == productId && 
                               oi.Order.Status != OrderStatus.Delivered && 
                               oi.Order.Status != OrderStatus.Cancelled);

            if (inActiveOrders)
            {
                return (false, "Cannot delete product while it is part of pending or in-transit orders.");
            }

            _fileUploadService.DeleteFile(product.ImageUrl);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AdminDeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            _fileUploadService.DeleteFile(product.ImageUrl);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                .Where(p => p.AvailableQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
