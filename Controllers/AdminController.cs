using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Helpers;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.Enum;
using _2tlob.ViewModels.Admin;

namespace _2tlob.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISellerService _sellerService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ISellerService sellerService)
        {
            _context = context;
            _userManager = userManager;
            _sellerService = sellerService;
        }

        [HttpGet]
        [Route("/Admin")]
        [Route("/Admin/Index")]
        [Route("/Admin/Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Efficient database queries using CountAsync and SumAsync
            var customerRoleUsers = await _userManager.GetUsersInRoleAsync("Customer");
            var sellerRoleUsers = await _userManager.GetUsersInRoleAsync("Seller");

            // Sellers keep the "Customer" role too (by design, so they can still
            // shop/order), so a straight Count of the Customer role list would
            // double-count every seller as a customer as well. Exclude anyone
            // who is also a Seller so the two stat cards don't overlap.
            var sellerIds = sellerRoleUsers.Select(u => u.Id).ToHashSet();
            int totalCustomers = customerRoleUsers.Count(u => !sellerIds.Contains(u.Id));
            int totalSellers = sellerRoleUsers.Count;
            int totalProducts = await _context.Products.CountAsync();
            int totalOrders = await _context.Orders.CountAsync();
            int pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            int pendingSellerRequests = await _context.SellerRequests.CountAsync(r => r.Status == RequestStatus.Pending);

            decimal totalRevenue = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            var recentSellerRequests = await _context.SellerRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.RequestedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalSellers = totalSellers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                PendingSellerRequests = pendingSellerRequests,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders,
                RecentSellerRequests = recentSellerRequests
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users(string? search, string? role, UserStatus? status)
        {
            var query = _userManager.Users
                .Include(u => u.Products)
                .Include(u => u.Orders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => (u.Email != null && u.Email.ToLower().Contains(term)) ||
                                         u.FullName.ToLower().Contains(term));
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var userList = new List<UserItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = RoleHelper.GetDisplayRole(roles);

                if (!string.IsNullOrEmpty(role) && !roles.Contains(role))
                {
                    continue;
                }

                userList.Add(new UserItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Role = userRole,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    ProductCount = user.Products.Count,
                    OrderCount = user.Orders.Count,
                    IsAdmin = roles.Contains("Admin")
                });
            }

            var viewModel = new UserManagementViewModel
            {
                Users = userList,
                SearchTerm = search,
                SelectedRole = role,
                SelectedStatus = status
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            // Protect root admin from suspension
            if (await _userManager.IsInRoleAsync(user, "Admin") && user.Email == "admin@alasaree3.com")
            {
                TempData["Error"] = "Cannot suspend the primary System Administrator.";
                return RedirectToAction(nameof(Users));
            }

            // Prevent self-suspension
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot suspend your own administrative account.";
                return RedirectToAction(nameof(Users));
            }

            user.Status = user.Status == UserStatus.Active ? UserStatus.Suspended : UserStatus.Active;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"User {user.FullName} status updated to {user.Status}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> SellerRequests()
        {
            var requests = await _sellerService.GetAllRequestsAsync();
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSellerRequest(int id)
        {
            var result = await _sellerService.ApproveRequestAsync(id);

            if (result.Success)
            {
                TempData["Success"] = "Seller application approved. The user is now assigned the Seller role.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to approve seller request.";
            }

            return RedirectToAction(nameof(SellerRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSellerRequest(int id, string? adminNotes)
        {
            var result = await _sellerService.RejectRequestAsync(id, adminNotes);

            if (result.Success)
            {
                TempData["Info"] = "Seller application has been rejected.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to reject seller request.";
            }

            return RedirectToAction(nameof(SellerRequests));
        }

        [HttpGet]
        public async Task<IActionResult> ContactMessages(ContactMessageStatus? status)
        {
            var query = _context.ContactMessages
                .Include(c => c.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var messages = await query
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkContactMessageInProgress(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                TempData["Error"] = "Message not found.";
                return RedirectToAction(nameof(ContactMessages));
            }

            message.Status = ContactMessageStatus.InProgress;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message marked as in progress.";
            return RedirectToAction(nameof(ContactMessages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveContactMessage(int id, string? adminNotes)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                TempData["Error"] = "Message not found.";
                return RedirectToAction(nameof(ContactMessages));
            }

            message.Status = ContactMessageStatus.Resolved;
            message.ResolvedAt = DateTime.UtcNow;
            message.AdminNotes = adminNotes;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message marked as resolved.";
            return RedirectToAction(nameof(ContactMessages));
        }
    }
}
