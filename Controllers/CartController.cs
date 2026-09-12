using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Checkout;

namespace _2tlob.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: /Cart
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetCartAsync(customerId);
            return View(cart);
        }

        // POST: /Cart/AddToCart
        // Dual-mode action: a plain <form> post (includes returnUrl) redirects back to the page,
        // while an AJAX fetch (no returnUrl, used on the Product Details page) gets JSON back.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            var customerId = _userManager.GetUserId(User)!;
            var result = await _cartService.AddToCartAsync(customerId, productId, quantity <= 0 ? 1 : quantity);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData[result.Success ? "Success" : "Error"] = result.Success
                    ? "Item added to your cart."
                    : result.ErrorMessage;
                return LocalRedirect(returnUrl);
            }

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            var cartCount = await _cartService.GetCartItemCountAsync(customerId);
            return Json(new { cartCount });
        }

        // POST: /Cart/IncreaseQty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseQty(int cartItemId, string? returnUrl = null)
        {
            var customerId = _userManager.GetUserId(User)!;
            var result = await _cartService.IncreaseQuantityAsync(customerId, cartItemId);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction(nameof(Index)) : LocalRedirect(returnUrl);
        }

        // POST: /Cart/DecreaseQty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseQty(int cartItemId, string? returnUrl = null)
        {
            var customerId = _userManager.GetUserId(User)!;
            var result = await _cartService.DecreaseQuantityAsync(customerId, cartItemId);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction(nameof(Index)) : LocalRedirect(returnUrl);
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId, string? returnUrl = null)
        {
            var customerId = _userManager.GetUserId(User)!;
            var result = await _cartService.RemoveFromCartAsync(customerId, cartItemId);
            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? "Item removed from your cart."
                : result.ErrorMessage;
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction(nameof(Index)) : LocalRedirect(returnUrl);
        }

        // GET: /Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var customerId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetCartAsync(customerId);

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty. Add some products before checking out.";
                return RedirectToAction(nameof(Index));
            }

            if (cart.HasInvalidItems)
            {
                TempData["Error"] = "Some items in your cart are out of stock or exceed the available quantity. Please fix your cart before checking out.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CheckoutViewModel { Cart = cart });
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var customerId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetCartAsync(customerId);
            model.Cart = cart;

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty. Add some products before checking out.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _orderService.CheckoutAsync(customerId, model);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage ?? "Checkout failed. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction(nameof(OrderConfirmation), new { id = result.OrderId });
        }

        // GET: /Cart/OrderConfirmation/{id}
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var customerId = _userManager.GetUserId(User)!;
            var details = await _orderService.GetOrderDetailsAsync(id, customerId, isSeller: false, isAdmin: false);

            if (details == null)
            {
                return NotFound();
            }

            var vm = new OrderConfirmationViewModel
            {
                OrderId = details.Order.Id,
                OrderDate = details.Order.OrderDate,
                TotalAmount = details.Order.TotalAmount,
                Status = details.Order.Status,
                ShippingAddress = details.Order.ShippingAddress,
                City = details.Order.City,
                PostalCode = details.Order.PostalCode,
                PhoneNumber = details.Order.PhoneNumber,
                Items = details.Items.ToList()
            };

            return View(vm);
        }
    }
}
