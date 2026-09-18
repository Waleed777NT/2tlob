using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Enum;
using _2tlob.Models;
using _2tlob.ViewModels.Review;

namespace _2tlob.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> AddReview(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var hasDeliveredOrder = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId
                             && oi.Order.CustomerId == CurrentUserId
                             && oi.Order.Status == OrderStatus.Delivered);

            if (!hasDeliveredOrder)
            {
                TempData["ErrorMessage"] = "You can only review products that you have purchased and received.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // A customer can have only ONE review per product, but they can come back
            // and change it any time (e.g. after ordering it again) instead of being
            // locked into whatever they wrote the first time.
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.CustomerId == CurrentUserId);

            var model = new AddReviewViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl ?? "/images/products/default.jpg",
                IsEditing = existingReview != null,
                Rating = existingReview?.Rating ?? 5,
                Comment = existingReview?.Comment ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hasDeliveredOrder = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == model.ProductId
                             && oi.Order.CustomerId == CurrentUserId
                             && oi.Order.Status == OrderStatus.Delivered);

            if (!hasDeliveredOrder)
            {
                TempData["ErrorMessage"] = "You can only review products that you have purchased and received.";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == model.ProductId && r.CustomerId == CurrentUserId);

            if (existingReview != null)
            {
                // Update the existing review in place instead of creating a duplicate.
                existingReview.Rating = model.Rating;
                existingReview.Comment = model.Comment;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your review has been updated.";
            }
            else
            {
                var review = new Review
                {
                    ProductId = model.ProductId,
                    CustomerId = CurrentUserId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you! Your review has been submitted.";
            }

            return RedirectToAction("Details", "Products", new { id = model.ProductId });
        }
    }
}