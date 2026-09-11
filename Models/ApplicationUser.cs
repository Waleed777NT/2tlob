using _2tlob.Enum;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<SellerRequest> SellerRequests { get; set; } = new List<SellerRequest>();
        public virtual Cart? Cart { get; set; }
        public virtual Wishlist? Wishlist { get; set; }
    }
}
