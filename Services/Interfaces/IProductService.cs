using _2tlob.Models;
using _2tlob.ViewModels.Product;

namespace _2tlob.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel> GetFilteredProductsAsync(string? search, int? categoryId, string? sortBy, int page = 1, int pageSize = 12, string? currentUserId = null);
        Task<ProductDetailsViewModel?> GetProductDetailsAsync(int id, string? currentUserId);
        Task<IEnumerable<Product>> GetProductsBySellerAsync(string sellerId);
        Task<Product?> GetProductForEditAsync(int id, string sellerId);
        Task<(bool Success, string? ErrorMessage)> CreateProductAsync(ProductCreateViewModel model, string sellerId);
        Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(ProductEditViewModel model, string sellerId);
        Task<(bool Success, string? ErrorMessage)> UpdateStockAsync(int productId, string sellerId, int newQuantity);
        Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(int productId, string sellerId);
        Task<(bool Success, string? ErrorMessage)> AdminDeleteProductAsync(int productId);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8);
    }
}