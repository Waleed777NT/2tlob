using _2tlob.Data;
using _2tlob.DTOs.Seller;
using _2tlob.Enum;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Order;
using _2tlob.ViewModels.Seller;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _2tlob.Services.Implementations
{
    public class SellerService : ISellerService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool Success, string? ErrorMessage)> SubmitRequestAsync(string userId, SellerRequestCreateViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User account not found.");
            }

            if (await _userManager.IsInRoleAsync(user, "Seller") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return (false, "You are already a registered Seller or Administrator.");
            }

            bool hasPending = await _context.SellerRequests
                .AnyAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);

            if (hasPending)
            {
                return (false, "You already have a pending seller application under review.");
            }

            var request = new SellerRequest
            {
                UserId = userId,
                BusinessName = model.BusinessName.Trim(),
                Reason = model.Reason.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                Status = RequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.SellerRequests.Add(request);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> HasPendingRequestAsync(string userId)
        {
            return await _context.SellerRequests
                .AnyAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);
        }

        public async Task<SellerRequest?> GetUserPendingRequestAsync(string userId)
        {
            return await _context.SellerRequests
                .Where(r => r.UserId == userId && r.Status == RequestStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SellerRequest>> GetPendingRequestsAsync()
        {
            return await _context.SellerRequests
                .Include(r => r.User)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderBy(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<SellerRequest>> GetAllRequestsAsync()
        {
            return await _context.SellerRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int requestId)
        {
            var request = await _context.SellerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return (false, "Seller request not found.");
            }

            if (request.Status != RequestStatus.Pending)
            {
                return (false, "This request has already been processed.");
            }

            var user = request.User ?? await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return (false, "Associated user account not found.");
            }

            // Verify the user is not already a Seller
            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                return (false, "User is already assigned the Seller role.");
            }

            // Keep the Customer role so the user can still shop, order, and use the
            // wishlist/cart after becoming a Seller. Sellers gain the Seller role in
            // ADDITION to Customer, rather than replacing it.

            // Assign the Seller role to the SAME ApplicationUser
            var roleResult = await _userManager.AddToRoleAsync(user, "Seller");
            if (!roleResult.Succeeded)
            {
                return (false, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            request.Status = RequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId, string? adminNotes)
        {
            var request = await _context.SellerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return (false, "Seller request not found.");
            }

            if (request.Status != RequestStatus.Pending)
            {
                return (false, "This request has already been processed.");
            }

            request.Status = RequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.AdminNotes = adminNotes?.Trim();

            // Note: User role remains unchanged (Customer stays Customer)
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<SellerDashboardViewModel> GetSellerDashboardAsync(string sellerId)
        {
            var productsQuery = _context.Products.Where(p => p.SellerId == sellerId);

            int totalProducts = await productsQuery.CountAsync();
            int lowStockProducts = await productsQuery.CountAsync(p => p.AvailableQuantity <= 5);

            var recentProducts = await productsQuery
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            var sellerOrderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Customer)
                .Include(oi => oi.Product)
                .Where(oi => oi.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .AsNoTracking()
                .ToListAsync();

            int totalOrdersCount = sellerOrderItems.Select(oi => oi.OrderId).Distinct().Count();

            // Total sales = sum of Quantity * UnitPrice for non-cancelled orders
            decimal totalSalesRevenue = sellerOrderItems
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .Sum(oi => oi.Quantity * oi.UnitPrice);

            var recentOrders = sellerOrderItems
                .Take(5)
                .Select(oi => new SellerOrderItemDto
                {
                    OrderItemId = oi.Id,
                    OrderId = oi.OrderId,
                    OrderDate = oi.Order.OrderDate,
                    CustomerName = oi.Order.Customer.FullName,
                    CustomerEmail = oi.Order.Customer.Email ?? string.Empty,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    OrderStatus = oi.Order.Status,
                    ShippingAddress = oi.Order.ShippingAddress,
                    City = oi.Order.City,
                    PhoneNumber = oi.Order.PhoneNumber
                })
                .ToList();

            return new SellerDashboardViewModel
            {
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                TotalOrdersCount = totalOrdersCount,
                TotalSalesRevenue = totalSalesRevenue,
                RecentProducts = recentProducts,
                RecentOrders = recentOrders
            };
        }

        public async Task<SellerOrderListViewModel> GetSellerOrdersAsync(string sellerId)
        {
            var sellerOrderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Customer)
                .Include(oi => oi.Product)
                .Where(oi => oi.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .AsNoTracking()
                .Select(oi => new SellerOrderItemDto
                {
                    OrderItemId = oi.Id,
                    OrderId = oi.OrderId,
                    OrderDate = oi.Order.OrderDate,
                    CustomerName = oi.Order.Customer.FullName,
                    CustomerEmail = oi.Order.Customer.Email ?? string.Empty,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    OrderStatus = oi.Order.Status,
                    ShippingAddress = oi.Order.ShippingAddress,
                    City = oi.Order.City,
                    PhoneNumber = oi.Order.PhoneNumber
                })
                .ToListAsync();

            return new SellerOrderListViewModel
            {
                Items = sellerOrderItems
            };
        }
    }
}
