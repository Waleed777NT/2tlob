using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
