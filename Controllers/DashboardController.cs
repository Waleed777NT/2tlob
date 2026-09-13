using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Enum;
using _2tlob.Models;
using _2tlob.ViewModels.Admin;
using _2tlob.ViewModels.Seller;

namespace _2tlob.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");

            var model = new AdminDashboardViewModel
            {
                TotalCustomers = customers.Count,
                TotalSellers = sellers.Count,
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),

                
                TotalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,

                
                RecentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),

                RecentSellerRequests = await _context.SellerRequests
                    .Include(sr => sr.User)
                    .OrderByDescending(sr => sr.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Seller()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var totalProducts = await _context.Products
                .CountAsync(p => p.SellerId == currentUserId);

            var lowStockProducts = await _context.Products
                .CountAsync(p => p.SellerId == currentUserId && p.AvailableQuantity <= 5);

           
            var totalOrdersCount = await _context.OrderItems
                .Where(oi => oi.SellerId == currentUserId)
                .Select(oi => oi.OrderId)
                .Distinct()
                .CountAsync();

            var totalSalesRevenue = await _context.OrderItems
                .Where(oi => oi.SellerId == currentUserId)
                .SumAsync(oi => (decimal?)(oi.UnitPrice * oi.Quantity)) ?? 0m;

            
            var recentProducts = await _context.Products
                .Where(p => p.SellerId == currentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var model = new SellerDashboardViewModel
            {
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                TotalOrdersCount = totalOrdersCount,
                TotalSalesRevenue = totalSalesRevenue,
                RecentProducts = recentProducts
            };

            return View(model);
        }
    }
}