using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _2tlob.Models;

namespace _2tlob.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<SellerRequest> SellerRequests => Set<SellerRequest>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category & Product configuration
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
            });

            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Seller)
                    .WithMany(u => u.Products)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.CategoryId);
                entity.HasIndex(p => p.SellerId);
                entity.HasIndex(p => p.Price);
            });

            // Cart & CartItem configuration
            builder.Entity<Cart>(entity =>
            {
                entity.HasOne(c => c.Customer)
                    .WithOne(u => u.Cart)
                    .HasForeignKey<Cart>(c => c.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.CustomerId).IsUnique();
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            });

            // Wishlist & WishlistItem configuration
            builder.Entity<Wishlist>(entity =>
            {
                entity.HasOne(w => w.Customer)
                    .WithOne(u => u.Wishlist)
                    .HasForeignKey<Wishlist>(w => w.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => w.CustomerId).IsUnique();
            });

            builder.Entity<WishlistItem>(entity =>
            {
                entity.HasOne(wi => wi.Wishlist)
                    .WithMany(w => w.Items)
                    .HasForeignKey(wi => wi.WishlistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wi => wi.Product)
                    .WithMany(p => p.WishlistItems)
                    .HasForeignKey(wi => wi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Prevent duplicate wishlist items
                entity.HasIndex(wi => new { wi.WishlistId, wi.ProductId }).IsUnique();
            });

            // Order & OrderItem configuration
            builder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(o => o.Customer)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(o => o.CustomerId);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.Status);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(oi => oi.Seller)
                    .WithMany()
                    .HasForeignKey(oi => oi.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(oi => oi.SellerId);
                entity.HasIndex(oi => oi.OrderId);
            });

            // Review configuration
            builder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Customer)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique review per customer per product
                entity.HasIndex(r => new { r.CustomerId, r.ProductId }).IsUnique();
            });

            // SellerRequest configuration
            builder.Entity<SellerRequest>(entity =>
            {
                entity.HasOne(sr => sr.User)
                    .WithMany(u => u.SellerRequests)
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(sr => sr.UserId);
                entity.HasIndex(sr => sr.Status);
            });

            // ContactMessage configuration
            builder.Entity<ContactMessage>(entity =>
            {
                entity.HasOne(cm => cm.User)
                    .WithMany()
                    .HasForeignKey(cm => cm.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(cm => cm.Status);
                entity.HasIndex(cm => cm.UserId);
                entity.HasIndex(cm => cm.CreatedAt);
            });
        }
    }
}
