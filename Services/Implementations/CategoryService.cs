using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Admin;

namespace _2tlob.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryViewModel>> GetAllWithCountsAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count,
                    CreatedAt = c.CreatedAt
                })
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(CategoryViewModel model)
        {
            bool exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == model.Name.Trim().ToLower());
            if (exists)
            {
                return (false, "A category with this name already exists.");
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(CategoryViewModel model)
        {
            var category = await _context.Categories.FindAsync(model.Id);
            if (category == null)
            {
                return (false, "Category not found.");
            }

            bool duplicateName = await _context.Categories
                .AnyAsync(c => c.Id != model.Id && c.Name.ToLower() == model.Name.Trim().ToLower());
            if (duplicateName)
            {
                return (false, "Another category with this name already exists.");
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description.Trim();

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            return !await _context.Products.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return (false, "Category not found.");
            }

            bool hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                return (false, "Cannot delete this category because it contains active products. Please reassign or delete the products first.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
