using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using _2tlob.Models;
using _2tlob.Services.Interfaces;

namespace _2tlob.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(IProductService productService, UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, string? sortBy, int page = 1)
        {
            var currentUserId = _userManager.GetUserId(User);
            var model = await _productService.GetFilteredProductsAsync(search, categoryId, sortBy, page, currentUserId: currentUserId);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User); // null if not logged in
            var model = await _productService.GetProductDetailsAsync(id, currentUserId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}