using _2tlob.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _2tlob.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public virtual ApplicationUser Customer { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation property
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
