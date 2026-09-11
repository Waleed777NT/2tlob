using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace _2tlob.ViewModels.Product
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 200 characters.")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Available quantity is required.")]
        [Range(0, 1000000, ErrorMessage = "Quantity cannot be negative.")]
        [Display(Name = "Stock Quantity")]
        public int AvailableQuantity { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Change Product Image")]
        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
