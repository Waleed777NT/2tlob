using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _2tlob.Models;
using _2tlob.Services.Interfaces;

namespace _2tlob.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
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
