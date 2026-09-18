<div align="center">

<!-- Animated wave banner -->
<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0D1B12,100:145A32&height=180&section=header&text=2tlob&fontSize=48&fontColor=ffffff&animation=fadeIn&fontAlignY=38&desc=E-Commerce%20Marketplace%20%7C%20ASP.NET%20Core%20MVC&descAlignY=58&descSize=18" width="100%"/>

<!-- Animated typing intro -->
<a href="#">
  <img src="https://readme-typing-svg.demolab.com/?font=Fira+Code&weight=600&size=22&pause=1000&color=2ECC71&center=true&vCenter=true&width=650&lines=Multi-vendor+marketplace+%7C+ASP.NET+Core+MVC;Customers+%C2%B7+Sellers+%C2%B7+Admins+in+one+platform" alt="Typing SVG" />
</a>

<br>

<a href="https://github.com/Waleed777NT/2tlob">
  <img src="https://img.shields.io/badge/REPOSITORY-145A32?style=for-the-badge&logo=github&logoColor=white">
</a>
<a href="#getting-started">
  <img src="https://img.shields.io/badge/GETTING_STARTED-0D1B12?style=for-the-badge&logo=dotnet&logoColor=white">
</a>
<a href="#features">
  <img src="https://img.shields.io/badge/FEATURES-0D1B12?style=for-the-badge&logo=readme&logoColor=white">
</a>

<br><br>

<img src="https://img.shields.io/badge/.NET_9-0D1B12?style=for-the-badge&logo=.net&logoColor=white" alt=".NET 9">
<img src="https://img.shields.io/badge/ASP.NET_Core_MVC-145A32?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC">
<img src="https://img.shields.io/badge/EF_Core-145A32?style=for-the-badge&logo=dotnet&logoColor=white" alt="EF Core">
<img src="https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white" alt="SQL Server">
<img src="https://img.shields.io/badge/Bootstrap_5-8511FA?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap 5">
<img src="https://img.shields.io/badge/Identity-0D1B12?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Identity">

<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>

</div>

## 📖 Overview

**2tlob** is a multi-vendor e-commerce web application where customers can browse and buy products from independent sellers, and sellers and admins manage the catalog, orders, and the marketplace itself — all built end-to-end with ASP.NET Core MVC, Entity Framework Core, and SQL Server.

Built as the graduation project for the **ITI .NET Summer Training Program**.

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>
</div>

## 🧰 Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 9)
- **Data access:** Entity Framework Core 9 (Code-First) + SQL Server
- **Auth:** ASP.NET Core Identity — role-based (`Admin`, `Seller`, `Customer`)
- **Frontend:** Razor Views, Bootstrap 5, Bootstrap Icons, jQuery
- **AI assistant:** In-app chatbot backed by the Gemini API, with a rule-based fallback when no API key is configured

## 👥 User Roles

| Role | Description |
|---|---|
| **Customer** | Browses the catalog, shops, orders, reviews, and wishlists products |
| **Seller** | An approved account that lists and manages its own products and fulfills its own orders |
| **Administrator** | Oversees users, sellers, categories, products, and orders across the whole marketplace |

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>
</div>

## ✨ Features

### 🛍️ Customer
- Register, log in / log out, and manage their profile
- Browse and search the product catalog
- Filter products by category and sort by price
- View product details, including ratings and reviews
- Shopping cart: add products, change quantities, remove items, see the live total
- Checkout, with the system blocking orders that exceed available stock
- Place orders and view order history / order details
- Cancel an order while it's still in a cancellable state
- Add or remove products from a wishlist
- Leave a rating + comment review — restricted to products from a **delivered** order, one review per product (editable afterward)

### 📦 Products & Catalog
- Every product carries a name, description, price, available quantity, category, seller, and image
- Public catalog with search, category filtering, price sorting, and pagination

### 🏪 Seller
- Submit a request to become a seller, reviewed by an admin before approval
- Add, edit, and delete their own products
- Update stock quantity for their products
- View orders that contain their products and update those orders' status
- Seller dashboard: their products, their orders, and their sales
- Ownership is enforced everywhere — a seller can only ever manage their own products and orders

### 🛡️ Administrator
- Review, approve, or reject seller requests
- Manage users: view, activate, and suspend accounts
- Manage categories: add, edit, and delete
- Manage products: view all listings, remove inappropriate ones
- View all orders and their details across every seller
- Review incoming contact/support messages, with status tracking (new / in progress / resolved)
- Admin dashboard: total customers, total sellers, total products, total orders, and pending orders

### 🚀 Beyond the core spec
- **AI chatbot assistant** (Gemini API, with an offline rule-based fallback) as a lightweight in-app helper
- **Contact/support inbox** for the admin team to track and resolve incoming messages

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>
</div>

## ✅ Requirements Coverage

Built against the project's graduation requirements document — every core module below is implemented:

- [x] Customer accounts (register, login/logout, profile)
- [x] Product browsing, search, category filter, price sort, product details
- [x] Shopping cart (add / update / remove, total, stock-safe checkout)
- [x] Orders (create, history, details, cancel) with status lifecycle: `Pending → Confirmed → Shipped → Delivered` / `Cancelled`
- [x] Wishlist (add / remove)
- [x] Seller product management (add / edit / delete / update quantity, own-products only)
- [x] Seller order handling (view & update status of orders containing their products)
- [x] Seller approval workflow (request → admin approve/reject)
- [x] Category management (admin add / edit / delete)
- [x] Reviews (rating + comment, shown with overall product rating)
- [x] Admin user management (view / activate / suspend)
- [x] Admin product moderation (view / remove inappropriate products)
- [x] Admin order oversight (view all orders & details)
- [x] Admin dashboard (customers, sellers, products, orders, pending orders)
- [x] Seller dashboard (products, orders, sales)

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>
</div>

## 🚦 Getting Started

1. **Clone the repo**
   ```bash
   git clone https://github.com/Waleed777NT/2tlob.git
   cd 2tlob
   ```
2. **Configure the database** — set `ConnectionStrings:DefaultConnection` in `appsettings.json` (or `appsettings.Development.json`) to a reachable SQL Server / LocalDB instance.
3. **Run the app** — migrations are applied and the database is seeded automatically on startup:
   ```bash
   dotnet run
   ```
4. *(Optional)* Add a `Gemini:ApiKey` to your configuration to enable the AI-powered chatbot; without it, the chatbot falls back to rule-based replies.

### 🔑 Seeded accounts (development)

On first run the app seeds roles, an admin account, sample categories, sellers, customers, and products so the site has real content right away:

| Role | Email | Password |
|---|---|---|
| Admin | `adminMarketplace22@gmail.com` | `Admin@Password123!` |
| Seller | `seller1@2tlob.com` / `seller2@2tlob.com` | `Seller@123!` |
| Customer | `customer1@2tlob.com` / `customer2@2tlob.com` / `customer3@2tlob.com` | `Customer@123!` |

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>
</div>

## 🗂️ Project Structure

```
2tlob/
├── Controllers/       # MVC controllers (Account, Products, Cart, Orders, Seller, Admin*, ...)
├── Models/             # EF Core entities (Product, Order, Cart, Review, Category, ...)
├── ViewModels/         # Request/response shaping for views
├── Views/               # Razor views per controller
├── Services/           # Business logic (Interfaces + Implementations)
├── Data/                 # ApplicationDbContext + SeedData
├── Migrations/       # EF Core migrations
└── wwwroot/            # Static assets (CSS, JS, product images)
```

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=rect&color=0:0D1B12,100:145A32&height=3&width=100%" width="100%"/>
</div>

## 🤝 Team — G02, Team 1

| # | Contributor |
|---|---|
| 1 | Waleed Nashaat Yousef Ramadan |
| 2 | Ahmed Tamer Fikry Alnahal |
| 3 | Ahmed Kareem Abdelmonem Beltagy |
| 4 | Mariam Ehab Ali Elkharat |
| 5 | Menna Ibrahim Ahmed Awad |

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=waving&color=0:145A32,100:0D1B12&height=100&section=footer" width="100%"/>
</div>
