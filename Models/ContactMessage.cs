using _2tlob.Enum;
using System.ComponentModel.DataAnnotations;

namespace _2tlob.Models
{
    // Contact requests may come from anonymous visitors or from users who were just
    // signed out (e.g. suspended accounts), so UserId is intentionally optional.
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Nullable link back to the account, if the sender was a known/registered user.
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public ContactReason Reason { get; set; } = ContactReason.General;

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        [StringLength(1000)]
        public string? AdminNotes { get; set; }
    }
}
