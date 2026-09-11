using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _2tlob.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000.00)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000000)]
        public int AvailableQuantity { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = "/images/products/";

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        [Required]
        public string SellerId { get; set; } = string.Empty;
        public virtual ApplicationUser Seller { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
