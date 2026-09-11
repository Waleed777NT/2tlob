using _2tlob.Models;
using _2tlob.ViewModels.Admin;

namespace _2tlob.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<CategoryViewModel>> GetAllWithCountsAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(CategoryViewModel model);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(CategoryViewModel model);
        Task<bool> CanDeleteAsync(int id);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
