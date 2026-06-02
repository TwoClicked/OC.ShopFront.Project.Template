# 🛍️ OC.LUAC Shop

A full-stack, multilingual e-commerce web application built with **ASP.NET Core 9** and **Blazor Server**. It supports customer registration and login, product browsing, shopping cart, checkout with vouchers and dynamic shipping zones, order management, and a full admin dashboard.

---

## 📋 Table of Contents

- [🏗️ Architecture](#architecture)
- [⚙️ Tech Stack](#tech-stack)
- [✨ Features](#features)
- [📁 Project Structure](#project-structure)
- [🚀 Getting Started](#getting-started)
- [🔧 Configuration](#configuration)
- [▶️ Running the App](#running-the-app)
- [☁️ Deployment](#deployment)
- [📡 API Reference](#api-reference)

---

## 🏗️ Architecture

The solution follows a clean **layered architecture** split into five projects:

```
OC.LUAC.ApiLayer      → ASP.NET Core Web API (controllers, JWT auth, SignalR hub)
OC.LUAC.UiLayer       → Blazor Server frontend (pages, components, services)
OC.LUAC.ServiceLayer  → Business logic and interfaces
OC.LUAC.DataLayer     → Entity Framework Core DbContext and migrations
OC.LUAC.ObjectLayer   → Domain models, DTOs, enums
```

The UI communicates with the API over HTTP. Customers authenticate via **cookie-based auth** (set by the UI layer); admins authenticate via **JWT bearer tokens**.

---

## ⚙️ Tech Stack

| Layer | Technology |
|---|---|
| 🖥️ Backend API | ASP.NET Core 9, C# |
| 🎨 Frontend | Blazor Server (.NET 9) |
| 🗄️ Database | SQL Server (EF Core 9) |
| 🔐 Authentication | Cookie auth (customers) + JWT (admin) |
| ⚡ Real-time | SignalR |
| 📄 PDF generation | QuestPDF (Community) |
| 📧 Email | MailKit / MimeKit (SMTP) |
| 💾 Local storage | Blazored.LocalStorage |
| ☁️ Hosting | Azure App Service (Windows) |
| 🌍 Localization | `en` / `de` (resource files) |

---

## ✨ Features

### 🛒 Storefront
- Product listing with search, category filter, featured flag, and pagination
- Product detail page with size variants and stock indicators
- Shopping cart (persisted in local storage)
- Checkout with:
  - Shipping address + optional separate billing address
  - Voucher code validation (percentage, fixed amount, free shipping)
  - Dynamic shipping cost per destination country/zone
  - Free-shipping threshold
- Order confirmation email (HTML + PDF invoice attachment, sent in customer's language)
- Guest checkout supported

### 👤 Customer Account
- Registration and login
- Order history
- Profile management

### 🔧 Admin Dashboard
- Product CRUD (images, variants, stock)
- Order management (view open/all orders, mark as shipped with tracking)
- Stock action log
- Voucher management (create, activate/deactivate, set limits and dates)
- Shipping zone management (countries, base cost, free-shipping threshold)
- Customer list and account management
- Real-time notifications via SignalR

---

## 📁 Project Structure

```
/
├── OC.LUAC.ApiLayer/
│   ├── Controllers/          # ProductController, OrderController, ShippingZoneController, ...
│   ├── Hubs/                 # SignalR hub
│   ├── Startup/              # DI registration helpers
│   └── appsettings.json
│
├── OC.LUAC.UiLayer/
│   ├── Components/Pages/     # Blazor pages (Checkout, AdminDash, ProductDetail, ...)
│   ├── Services/             # AuthService, AdminAuthService, CartService, ...
│   ├── wwwroot/              # Static assets, app.js
│   └── Program.cs
│
├── OC.LUAC.ServiceLayer/
│   ├── Interfaces/           # IProductService, IOrderService, IVoucherService, ...
│   └── Services/             # Concrete implementations
│
├── OC.LUAC.DataLayer/
│   ├── AppDbContext.cs
│   └── Migrations/
│
└── OC.LUAC.ObjectLayer/
    ├── Entities/             # Product, ProductVariant, ProductImage, Category, ...
    ├── Orders/               # Order, OrderItem, ShippingZone, ...
    └── Accounts/             # Customer, AdminUser
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or Azure) — a LocalDB instance works for development
- A Gmail account (or other SMTP provider) for order emails

### Clone and restore

```bash
git clone https://github.com/TwoClicked/OC.ShopFront.Project.Template.git
cd OC.ShopFront.Project.Template
dotnet restore
```

---

## 🔧 Configuration

Both `OC.LUAC.ApiLayer` and `OC.LUAC.UiLayer` have their own `appsettings.json`.

Sensitive values are **never stored in files** — use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:

```bash
cd OC.LUAC.ApiLayer
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<your-secret-key-min-32-chars>"
dotnet user-secrets set "AdminAuth:Email" "admin@yourdomain.com"
dotnet user-secrets set "AdminAuth:Password" "<admin-password>"
dotnet user-secrets set "Email:SmtpUser" "your@gmail.com"
dotnet user-secrets set "Email:SmtpPass" "<gmail-app-password>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=OCLUACShopDB;Trusted_Connection=True;"
```

### Apply database migrations

```bash
cd OC.LUAC.ApiLayer
dotnet ef database update --project ../OC.LUAC.DataLayer
```

---

## ▶️ Running the App

Open two terminals — the API and UI must run simultaneously:

```bash
# Terminal 1 — API
cd OC.LUAC.ApiLayer
dotnet run

# Terminal 2 — UI
cd OC.LUAC.UiLayer
dotnet run
```

Then open `https://localhost:7273` in your browser. 🌐

> Swagger UI is available at `https://localhost:7299/swagger`

---

## ☁️ Deployment

Both projects are configured for **Azure App Service (Windows)**:

| | Name |
|---|---|
| 🖥️ API App Service | `luac-api-win` |
| 🎨 UI App Service | `luac-ui-win` |
| 📦 Resource group | `luac-shop-rg` |
| 🌍 Region | Germany West Central |

ARM templates for each service are in:
```
OC.LUAC.ApiLayer/Properties/ServiceDependencies/
OC.LUAC.UiLayer/Properties/ServiceDependencies/
```

For production, set the connection string to the Azure SQL instance and update `Frontend.BaseUrl` and the CORS policy in `ApiLayer/Program.cs` to match your UI App Service domain.

---

## 📡 API Reference

The API is documented via Swagger. Key endpoint groups:

| Group | Base path | Auth |
|---|---|---|
| 📦 Products | `GET /api/products` | Public |
| 🔍 Product detail | `GET /api/products/{id}` | Public |
| 🛒 Orders | `POST /api/orders` | Customer (cookie) |
| 🔧 Admin orders | `GET /api/orders/admin/open` | Admin (JWT) |
| 🚚 Shipping quote | `GET /api/shipping-zones/quote` | Public |
| 🗺️ Shipping zones | `GET/POST/PUT/DELETE /api/shipping-zones` | Admin (JWT) |
| 🎟️ Vouchers | `/api/vouchers` | Admin (JWT) |
| 👥 Customers | `/api/customers` | Mixed |

> All admin endpoints require `Authorization: Bearer <token>` obtained from the admin login endpoint.

---

## 📜 License

Private / proprietary. All rights reserved.
