using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Admin;

namespace _2tlob.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllWithCountsAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.CreateAsync(model);
            if (result.Success)
            {
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create category.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.UpdateAsync(model);
            if (result.Success)
            {
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update category.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (result.Success)
            {
                TempData["Success"] = "Category deleted successfully.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to delete category.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
