using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public virtual ApplicationUser Customer { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
