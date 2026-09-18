using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _2tlob.Models;
using _2tlob.Services.Interfaces;

namespace _2tlob.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(IWishlistService wishlistService, UserManager<ApplicationUser> userManager)
        {
            _wishlistService = wishlistService;
            _userManager = userManager;
        }

        // GET: /Wishlist
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = _userManager.GetUserId(User)!;
            var products = await _wishlistService.GetWishlistProductsAsync(customerId);
            return View(products);
        }

        // POST: /Wishlist/Toggle
        // Dual-mode: a plain <form> post (includes returnUrl) redirects back, an AJAX fetch
        // (no returnUrl, used on the Product Details page) gets JSON { inWishlist } back.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            var customerId = _userManager.GetUserId(User)!;
            bool wasInWishlist = await _wishlistService.IsInWishlistAsync(customerId, productId);

            (bool Success, string? ErrorMessage) result;
            bool nowInWishlist;

            if (wasInWishlist)
            {
                result = await _wishlistService.RemoveFromWishlistAsync(customerId, productId);
                nowInWishlist = !result.Success; // if the removal failed, it's still in there
            }
            else
            {
                result = await _wishlistService.AddToWishlistAsync(customerId, productId);
                nowInWishlist = result.Success;
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                if (!result.Success)
                {
                    TempData["Error"] = result.ErrorMessage;
                }
                else
                {
                    TempData["Success"] = nowInWishlist ? "Added to your wishlist." : "Removed from your wishlist.";
                }
                return LocalRedirect(returnUrl);
            }

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Json(new { inWishlist = nowInWishlist });
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var customerId = _userManager.GetUserId(User)!;
            var result = await _wishlistService.RemoveFromWishlistAsync(customerId, productId);

            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? "Removed from your wishlist."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }
    }
}
