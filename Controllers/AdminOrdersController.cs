using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.Enum;

namespace _2tlob.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("AdminOrders")]
        [Route("AdminOrders/Index")]
        [Route("Admin/Orders")]
        [Route("Admin/Orders/Index")]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        [Route("AdminOrders/Details/{id:int?}")]
        [Route("AdminOrders/ManageOrder/{id:int?}")]
        [Route("Admin/Orders/Details/{id:int?}")]
        [Route("Admin/Orders/ManageOrder/{id:int?}")]
        public async Task<IActionResult> Details(int? id, int? orderId)
        {
            int targetId = id ?? orderId ?? 0;
            if (targetId <= 0)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User)!;
            var orderDetails = await _orderService.GetOrderDetailsAsync(targetId, currentUserId, isSeller: false, isAdmin: true);

            if (orderDetails == null)
            {
                return NotFound();
            }

            return View("Details", orderDetails);
        }

        [HttpGet]
        [Route("AdminOrders/Manage/{id:int?}")]
        public Task<IActionResult> ManageOrder(int? id, int? orderId)
        {
            return Details(id, orderId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AdminOrders/UpdateStatus")]
        [Route("Admin/Orders/UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var currentUserId = _userManager.GetUserId(User)!;
            var result = await _orderService.UpdateOrderStatusAsync(orderId, status, currentUserId, isAdmin: true, isSeller: false);

            if (result.Success)
            {
                TempData["Success"] = $"Order #{orderId} status updated to {status}.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not update order status.";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
