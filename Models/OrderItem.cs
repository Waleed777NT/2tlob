using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _2tlob.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required]
        public string SellerId { get; set; } = string.Empty;
        public virtual ApplicationUser Seller { get; set; } = null!;

        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}
