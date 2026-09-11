using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; }
    }
}
