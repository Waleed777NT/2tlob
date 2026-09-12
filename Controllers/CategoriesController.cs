using Microsoft.AspNetCore.Mvc;
using _2tlob.Services.Interfaces;

namespace _2tlob.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CategoriesController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllWithCountsAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int id, string? search, string? sortBy, int page = 1)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var products = await _productService.GetFilteredProductsAsync(search, id, sortBy, page);
            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = id;

            return View(products);
        }
    }
}