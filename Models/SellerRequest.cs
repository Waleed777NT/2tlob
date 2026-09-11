using _2tlob.Enum;
using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    public class SellerRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
