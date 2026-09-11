using System.ComponentModel.DataAnnotations;

namespace _2tlob.ViewModels.Admin
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
