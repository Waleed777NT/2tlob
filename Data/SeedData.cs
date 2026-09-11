using Microsoft.AspNetCore.Identity;
using _2tlob.Models;
using _2tlob.Enum;

namespace _2tlob.Data
{
    // Minimal environment seeding: roles + one admin account, so the app is
    // testable from the start. Add your own sample sellers/customers/products
    // as you build out each module — no need to hardcode them here.
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Seller", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = configuration["AdminUserSeed:Email"] ?? "adminMarketplace22@gmail.com";
            var adminPassword = configuration["AdminUserSeed:Password"] ?? "Admin@Password123!";
            var adminFullName = configuration["AdminUserSeed:FullName"] ?? "System Administrator";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
