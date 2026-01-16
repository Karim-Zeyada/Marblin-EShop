# MARBLIN – Marble & Stone Luxury E-Shop

A premium e-commerce platform for selling handcrafted marble and stone artifacts. Built with **ASP.NET Core 8 MVC** following **Clean Architecture** principles.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green)

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
- ⚙️ **Site Settings** – Configure deposit %, payment info, homepage content

## 🏗️ Architecture

```
Marblin/
├── Marblin.Core/           # Domain entities, interfaces, specifications
├── Marblin.Application/    # Application services, DTOs, use cases
├── Marblin.Infrastructure/ # EF Core, repositories, external services
└── Marblin.Web/            # MVC controllers, views, presentation
```

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/marblin.git
   cd marblin
   ```

2. **Configure the database connection**
   
   Edit `Marblin.Web/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MarblinDB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Apply database migrations**
   ```bash
   cd Marblin.Web
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Store: `https://localhost:5001`
   - Admin: `https://localhost:5001/Admin`

### Default Admin Account
On first run, the database is seeded with a default admin:
- **Email:** `admin@marblin.com`
- **Password:** `Admin@123!`

> ⚠️ **Change this password immediately in production!**

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core 8 MVC |
| Database | SQL Server + Entity Framework Core |
| Auth | ASP.NET Identity |
| Mapping | AutoMapper |
| Frontend | Razor Views, CSS, JavaScript |

## 📄 License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
