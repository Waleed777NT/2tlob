using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }

        public int WishlistId { get; set; }
        public virtual Wishlist Wishlist { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
