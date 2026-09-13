using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _2tlob.Data;
using _2tlob.Middleware;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Configure Authentication Cookie Security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;

    // Return 401 (instead of a redirect-to-login HTML page) for AJAX/fetch calls,
    // e.g. the Cart/AddToCart and Wishlist/Toggle fetch() calls on the Product Details page.
    options.Events.OnRedirectToLogin = context =>
    {
        var acceptHeader = context.Request.Headers["Accept"].ToString();
        bool wantsHtml = acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase);
        bool isAjax = string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (isAjax || (!string.IsNullOrEmpty(acceptHeader) && !wantsHtml))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// 4. Configure Anti-Forgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// 5. Register Business Services
// TODO (each member): as you build your Services/Interfaces + Services/Implementations,
// register them here, e.g.:
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ICartService, CartService>();
//builder.Services.AddScoped<IReviewService, ReviewService>();

// 6. Register MVC Controllers and Views
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

// 7. Auto-Apply Migrations and Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await SeedData.InitializeAsync(services, app.Configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing and seeding the database.");
    }
}

// 8. Configure HTTP Request Pipeline & Global Error Handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCodeHandler", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security Headers Middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Suspended User Check Middleware
app.UseMiddleware<UserStatusMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();