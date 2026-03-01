# MARBLIN – Marble & Stone Luxury E-Shop

**Proprietary Software - Developed for [Customer Name]**

A premium e-commerce platform for selling handcrafted marble and stone artifacts. Built with **ASP.NET Core 8 MVC**.

## ✨ Features

### Customer-Facing
- 🛒 **Guest Checkout** – No account required; track orders via Order ID + Email
- 💎 **Product Catalog** – Browse with filters, variants (material/size), and image galleries
- 💰 **Deposit System** – Configurable upfront payment percentage
- 📝 **Custom Requests** – Submit bespoke product inquiries with inspiration images
- 📦 **Order Tracking** – 4-step visual timeline (Pending → Production → Awaiting Balance → Shipped)

### Admin Panel
- 📊 **Dashboard** – Sales analytics and order status overview
- 📦 **Inventory Management** – Full CRUD for products, variants, categories
- ✅ **Order Workflow** – Verify payments and update order statuses
- 🎟️ **Coupon System** – Create and manage discount codes
- ⚙️ **Site Settings** – Configure deposit %, payment methods (Instapay/Vodafone Cash), shipping costs, and homepage content

## 🏗️ Architecture

```
Marblin/
├── Marblin.Core/           # Domain entities, interfaces, specifications
├── Marblin.Application/    # Application services, DTOs, use cases
├── Marblin.Infrastructure/ # EF Core, repositories, external services
└── Marblin.Web/            # MVC controllers, views, presentation
```

## 🚀 Deployment Guide

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server)

### Database Setup
1. **Configure Connection**:
   Update `Marblin.Web/appsettings.json` with the target SQL Server connection string.

2. **Apply Migrations**:
   Run the following command in the `Marblin.Web` directory to create the database schema:
   ```bash
   dotnet ef database update
   ```

### Default Admin Credentials
**Important:** Change these credentials immediately after deployment.
- **Email:** `admin@marblin.com`
- **Password:** `Admin@123!`

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core 8 MVC |
| Database | SQL Server + Entity Framework Core |
| Auth | ASP.NET Identity |

---
**Confidential & Proprietary.** Unauthorized copying or distribution is strictly prohibited.
