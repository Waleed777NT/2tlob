using _2tlob.Enum;
using _2tlob.Models;

namespace _2tlob.ViewModels.Admin
{
    public class UserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UserManagementViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new List<UserItemViewModel>();
        public string? SearchTerm { get; set; }
        public string? SelectedRole { get; set; }
        public UserStatus? SelectedStatus { get; set; }
    }
}
