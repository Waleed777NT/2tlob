using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using _2tlob.Enum;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Product;
using _2tlob.ViewModels.Seller;

namespace _2tlob.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        private readonly ISellerService _sellerService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;

        public SellerController(
            ISellerService sellerService,
            IProductService productService,
            IOrderService orderService,
            ICategoryService categoryService)
        {
            _sellerService = sellerService;
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        [ActionName("RequestSeller")]
        public async Task<IActionResult> RequestSellerGet()
        {
            if (User.IsInRole("Seller"))
            {
                return RedirectToAction(nameof(Dashboard));
            }

            var hasPending = await _sellerService.HasPendingRequestAsync(CurrentUserId);
            if (hasPending)
            {
                TempData["InfoMessage"] = "Your request to become a seller is currently pending review.";
                return View("RequestStatus");
            }

            return View("RequestToJoin", new SellerRequestCreateViewModel());
        }

        [HttpPost]
        [ActionName("RequestSeller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestSellerPost(SellerRequestCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("RequestToJoin", model);
            }

            var result = await _sellerService.SubmitRequestAsync(CurrentUserId, model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Your seller request has been submitted successfully.";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to submit request.");
            return View("RequestToJoin", model);
        }

        [HttpGet]
        public Task<IActionResult> RequestToJoin() => RequestSellerGet();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RequestToJoin(SellerRequestCreateViewModel model) => RequestSellerPost(model);

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _sellerService.GetSellerDashboardAsync(CurrentUserId);
            return View(dashboard);
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> MyProducts()
        {
            var products = await _productService.GetProductsBySellerAsync(CurrentUserId);
            return View(products);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateProduct()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            var result = await _productService.CreateProductAsync(model, CurrentUserId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(MyProducts));
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create product.");
            var allCategories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(allCategories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _productService.GetProductForEditAsync(id, CurrentUserId);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryService.GetAllAsync();
            var model = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == product.CategoryId
                })
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                model.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == model.CategoryId
                });
                return View(model);
            }

            var result = await _productService.UpdateProductAsync(model, CurrentUserId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction(nameof(MyProducts));
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update product.");
            var allCategories = await _categoryService.GetAllAsync();
            model.Categories = allCategories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            });
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 0)
            {
                TempData["ErrorMessage"] = "Quantity cannot be negative.";
                return RedirectToAction(nameof(MyProducts));
            }

            var result = await _productService.UpdateStockAsync(productId, CurrentUserId, quantity);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Stock quantity updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update stock quantity.";
            }

            return RedirectToAction(nameof(MyProducts));
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id, CurrentUserId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete product.";
            }

            return RedirectToAction(nameof(MyProducts));
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _sellerService.GetSellerOrdersAsync(CurrentUserId);
            return View(orders);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(
                orderId,
                status,
                CurrentUserId,
                isAdmin: false,
                isSeller: true);    

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Order status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update order status.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}