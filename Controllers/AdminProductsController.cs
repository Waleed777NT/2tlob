using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Services.Interfaces;

namespace _2tlob.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public AdminProductsController(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => (p.Name != null && p.Name.ToLower().Contains(term)) || 
                                         (p.Seller != null && p.Seller.FullName != null && p.Seller.FullName.ToLower().Contains(term)) ||
                                         (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.AdminDeleteProductAsync(id);

            if (result.Success)
            {
                TempData["Success"] = "Product has been removed by Administrator.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to remove product.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
