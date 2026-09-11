using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public virtual ApplicationUser Customer { get; set; } = null!;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 1000 characters.")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
