using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _2tlob.Models;
using _2tlob.Enum;

namespace _2tlob.Data
{
    
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

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

            // 3. Seed sample catalog (categories, sellers, customers, products)
            var categories = await SeedCategoriesAsync(context);
            var sellers = await SeedUsersAsync(userManager, "Seller", SampleSellers);
            await SeedUsersAsync(userManager, "Customer", SampleCustomers);
            await SeedProductsAsync(context, categories, sellers);
        }

        private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(ApplicationDbContext context)
        {
            var definitions = new (string Name, string Description)[]
            {
                ("Electronics", "Gadgets, devices and tech accessories."),
                ("Fashion & Accessories", "Clothing, footwear and accessories for everyone."),
                ("Home & Kitchen", "Furniture, cookware and everyday essentials for the home."),
                ("Books & Stationery", "Books, notebooks and stationery supplies.")
            };

            foreach (var (name, description) in definitions)
            {
                if (!await context.Categories.AnyAsync(c => c.Name == name))
                {
                    context.Categories.Add(new Category
                    {
                        Name = name,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            return await context.Categories.ToDictionaryAsync(c => c.Name);
        }

        private static async Task<List<ApplicationUser>> SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            string role,
            (string Email, string FullName, string Password)[] definitions)
        {
            var users = new List<ApplicationUser>();

            foreach (var (email, fullName, password) in definitions)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        EmailConfirmed = true,
                        Status = UserStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
                else if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                users.Add(user);
            }

            return users;
        }

        private static async Task SeedProductsAsync(
            ApplicationDbContext context,
            Dictionary<string, Category> categories,
            List<ApplicationUser> sellers)
        {
            if (await context.Products.AnyAsync())
            {
                return;
            }

            for (int i = 0; i < SampleProducts.Length; i++)
            {
                var seller = sellers[i / 5]; // 5 products per seller
                foreach (var p in SampleProducts[i])
                {
                    context.Products.Add(new Product
                    {
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        AvailableQuantity = p.Quantity,
                        ImageUrl = $"/images/products/{p.ImageFile}",
                        CategoryId = categories[p.CategoryName].Id,
                        SellerId = seller.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        // --- Sample data definitions -------------------------------------------------

        private static readonly (string Email, string FullName, string Password)[] SampleSellers = new[]
        {
            ("seller1@2tlob.com", "TechHub Store", "Seller@123!"),
            ("seller2@2tlob.com", "UrbanStyle Boutique", "Seller@123!")
        };

        private static readonly (string Email, string FullName, string Password)[] SampleCustomers = new[]
        {
            ("customer1@2tlob.com", "Sara Ahmed", "Customer@123!"),
            ("customer2@2tlob.com", "Omar Khaled", "Customer@123!"),
            ("customer3@2tlob.com", "Mona Yousef", "Customer@123!")
        };

        
        private static readonly (string Name, string Description, decimal Price, int Quantity, string CategoryName, string ImageFile)[][] SampleProducts = new[]
        {
            // TechHub Store (Electronics / Home & Kitchen)
            new (string, string, decimal, int, string, string)[]
            {
                ("Wireless Bluetooth Headphones", "Over-ear headphones with active noise cancellation and 30-hour battery life.", 49.99m, 50, "Electronics", "Wireless.jpeg"),
                ("Smart Fitness Watch", "Tracks heart rate, sleep and workouts, with a 7-day battery life.", 59.99m, 40, "Electronics", "watch.jpeg"),
                ("4K Action Camera", "Waterproof action camera with image stabilization, perfect for travel and sports.", 89.99m, 25, "Electronics", "cam.jpeg"),
                ("Stainless Steel Cookware Set", "10-piece non-stick cookware set for everyday cooking.", 74.99m, 30, "Home & Kitchen", "cookset.jpeg"),
                ("Electric Coffee Maker", "12-cup programmable coffee maker with keep-warm plate.", 39.99m, 35, "Home & Kitchen", "CoffeeMaker.jpeg")
            },
            // UrbanStyle Boutique (Fashion / Books & Stationery)
            new (string, string, decimal, int, string, string)[]
            {
                ("Men's Leather Jacket", "Genuine leather jacket with a slim, modern fit.", 129.99m, 20, "Fashion & Accessories", "LeatherJack.jpg"),
                ("Running Sneakers", "Lightweight, breathable sneakers built for daily runs.", 54.99m, 60, "Fashion & Accessories", "RunningSneak.jpg"),
                ("Canvas Travel Backpack", "Durable canvas backpack with laptop compartment.", 44.99m, 45, "Fashion & Accessories", "Travelbag.jpeg"),
                ("Hardcover Notebook Set", "Set of 3 hardcover ruled notebooks, A5 size.", 14.99m, 100, "Books & Stationery", "Noteset.jpg"),
                ("Bestseller Novel Bundle", "A curated bundle of 3 bestselling fiction novels.", 24.99m, 70, "Books & Stationery", "Novels.jpg")
            }
        };
    }
}

