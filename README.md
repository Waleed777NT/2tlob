# 2tlob — Multi-Vendor E-Commerce Platform (Graduation Project)

ASP.NET Core MVC (.NET 9) marketplace connecting Customers, Sellers, and an
Administrator, plus a simple chatbot. This repo is currently a **prepared
environment**, not a finished app: the data model, database, and auth
scaffolding are done; the controllers/views/services for each feature are
intentionally empty so the team builds them.

---

## 1. What's already set up (do not need to rebuild)

- **Models** (`Models/`) — `ApplicationUser`, `Product`, `Category`, `Cart`,
  `CartItem`, `Wishlist`, `WishlistItem`, `Order`, `OrderItem`, `Review`,
  `SellerRequest`, `ContactMessage`, plus enums (`OrderStatus`,
  `RequestStatus`, `UserStatus`, `ContactReason`, `ContactMessageStatus`).
- **Database** (`Data/ApplicationDbContext.cs`) — EF Core `IdentityDbContext`
  with all `DbSet`s and relationships/indexes configured.
- **Migrations** (`Migrations/`) — match the current models. Applied
  automatically on startup (`context.Database.MigrateAsync()` in
  `Program.cs`).
- **Identity & Roles** — ASP.NET Core Identity is wired up in `Program.cs`
  (cookie auth, password rules, lockout). `Data/SeedData.cs` seeds the three
  roles (`Admin`, `Seller`, `Customer`) and one admin account on startup.
- **Shared UI shell** (`Views/Shared/_Layout.cshtml`, `_LoginPartial.cshtml`,
  `_Alerts.cshtml`, `Error.cshtml`, `NotFound.cshtml`, `AccessDenied.cshtml`)
  — the nav bar, login/logout menu, and error pages already link to the
  exact controller/action names below, so the site "lights up" as each
  controller is built. No changes needed here to get started.
- **Middleware** — security headers and a suspended-user sign-out check.
- **Assets** — `wwwroot/` (Bootstrap-based `site.css`, `site.js`, product
  placeholder images).

## 2. What's intentionally empty (the team's work)

`Controllers/` (only `HomeController` exists, as a bare placeholder),
`Views/` (only the shared shell + a placeholder `Home/Index`), `Services/`
(deleted — interfaces + implementations to be created), `ViewModels/`
(deleted except `Common/ErrorViewModel`).

## 3. Getting started

1. Open `2tlob.csproj` in Visual Studio, or `cd` into the folder and run
   `dotnet restore`.
2. Update `appsettings.json` → `ConnectionStrings:DefaultConnection` to point
   at your own SQL Server / LocalDB instance.
3. Run the app (`dotnet run` or F5). On first run it applies migrations and
   seeds roles + an admin user automatically (see `AdminUserSeed` in
   `appsettings.json` for the seeded email/password — change it before
   sharing the repo).
4. Pull the repo, create a feature branch per module, and start building
   your section below.

## 4. Team task breakdown

Each member owns a vertical slice: model fields already exist, so this is
controllers + services + views + Razor pages.

### Member 1 — Identity, Roles & Admin User Management
- `AccountController` — Register / Login / Logout (Identity), `Profile`
- `AdminController` — `Dashboard`, `Users` (activate/suspend), `SellerRequests` (approve/reject), `ContactMessages`
- Views: `Views/Account/*`, `Views/Admin/*`

### Member 2 — Products & Categories
- `ProductController` — `Index` (browse/search/filter/sort), `Details`
- `AdminCategoriesController` — CRUD
- `AdminProductsController` — moderation (`Index`, remove inappropriate)
- Views: `Views/Product/*`, `Views/AdminCategories/*`, `Views/AdminProducts/*`
- Consider replacing the placeholder `Home/Index` with real product browsing, or keep it as a landing page and route "Browse Products" to `Product/Index` (nav already points there)

### Member 3 — Cart & Orders
- `CartController` — add/remove/update quantity, `Index`
- `OrderController` — `Checkout`, `MyOrders`, `Details`, `Cancel`
- `AdminOrdersController` — `Index`, `Details` (all orders)
- `WishlistController` — add/remove, `Index`
- Views: `Views/Cart/*`, `Views/Order/*`, `Views/AdminOrders/*`, `Views/Wishlist/*`

### Member 4 — Seller Panel & Reviews
- `SellerController` — `Dashboard`, `MyProducts`, `CreateProduct`, `EditProduct`, `MyOrders`, `UpdateOrderStatus`, `RequestSeller`
- `ReviewController` — `AddReview` (purchase-verified)
- Views: `Views/Seller/*`, review partial embedded in `Product/Details`

### Member 5 — Chatbot & Dashboards (Integration Lead)
- Chatbot controller + widget partial, injected into `_Layout.cshtml`
- Dashboard sections inside `AdminController.Dashboard` and `SellerController.Dashboard`
- `ContactController` — `Index`, `ThankYou`
- Branch merging, final integration testing, deployment

## 5. Notes
- The nav bar and login menu already reference all of the above
  controller/action names — a link will 404 until that controller exists,
  which is expected during development.
- Register your services in `Program.cs` (`TODO` comment marks where) as you
  build them.
