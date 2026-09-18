using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Models;
using _2tlob.Services.Interfaces;

namespace _2tlob.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _orderService = orderService;
            _userManager = userManager;
            _context = context;
        }

        // GET: /Orders  or  /Orders/MyOrders  -> customer's order history
        [HttpGet]
        [Route("Orders")]
        [Route("Orders/Index")]
        [Route("Orders/MyOrders")]
        public async Task<IActionResult> MyOrders()
        {
            var customerId = _userManager.GetUserId(User)!;
            var orders = await _orderService.GetCustomerOrdersAsync(customerId);
            return View(orders);
        }

        // GET: /Orders/Details/{id}
        [HttpGet]
        [Route("Orders/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var customerId = _userManager.GetUserId(User)!;
            var details = await _orderService.GetOrderDetailsAsync(id, customerId, isSeller: false, isAdmin: false);

            if (details == null)
            {
                return NotFound();
            }

            // Products already reviewed by this customer (from any order) -> hide the
            // "Set review" button for those items and show a "Reviewed" badge instead.
            var productIds = details.Items.Select(i => i.ProductId).Distinct().ToList();
            var reviewedProductIds = await _context.Reviews
                .Where(r => r.CustomerId == customerId && productIds.Contains(r.ProductId))
                .Select(r => r.ProductId)
                .ToListAsync();

            ViewBag.ReviewedProductIds = reviewedProductIds.ToHashSet();

            return View(details);
        }

        // POST: /Orders/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Orders/Cancel/{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var customerId = _userManager.GetUserId(User)!;
            var result = await _orderService.CancelOrderAsync(id, customerId, isAdmin: false);

            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? $"Order #{id} has been cancelled."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
