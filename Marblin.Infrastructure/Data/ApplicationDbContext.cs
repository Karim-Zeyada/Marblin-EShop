using Marblin.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Domain Entities
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<CustomRequest> CustomRequests => Set<CustomRequest>();
        public DbSet<CustomRequestImage> CustomRequestImages => Set<CustomRequestImage>();
        public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<Announcement> Announcements => Set<Announcement>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========== CATEGORY ==========
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ========== PRODUCT ==========
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.BasePrice).HasPrecision(18, 2);
                entity.Property(e => e.SalePrice).HasPrecision(18, 2);
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.IsSignaturePiece);
                entity.HasIndex(e => e.Availability);
                entity.HasIndex(e => e.IsFeaturedSale);
            });

            // ========== PRODUCT VARIANT ==========
            builder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Material).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Size).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PriceAdjustment).HasPrecision(18, 2);
                entity.Property(e => e.SKU).HasMaxLength(50);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");
            });

            // ========== PRODUCT IMAGE ==========
            builder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.AltText).HasMaxLength(200);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ORDER ==========
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AddressLine).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.DepositPercentage).HasPrecision(5, 2);
                entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.ReceiptImageUrl).HasMaxLength(500);

                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);

                // Ignore computed properties
                entity.Ignore(e => e.RemainingBalance);
                entity.Ignore(e => e.AmountDue);
            });

            // ========== ORDER ITEM ==========
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.VariantDescription).HasMaxLength(200);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Variant)
                    .WithMany()
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Ignore computed property
                entity.Ignore(e => e.LineTotal);
            });

            // ========== CUSTOM REQUEST ==========
            builder.Entity<CustomRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Dimensions).HasMaxLength(200);
                entity.Property(e => e.Material).HasMaxLength(100);
                entity.Property(e => e.BudgetRange).HasMaxLength(100);
                entity.Property(e => e.Timeline).HasMaxLength(100);
                entity.Property(e => e.Notes).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.AdminNotes).HasMaxLength(2000);

                entity.HasIndex(e => e.IsReviewed);
            });

            // ========== CUSTOM REQUEST IMAGE ==========
            builder.Entity<CustomRequestImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);

                entity.HasOne(e => e.CustomRequest)
                    .WithMany(cr => cr.Images)
                    .HasForeignKey(e => e.CustomRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== SITE SETTINGS ==========
            builder.Entity<SiteSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DepositPercentage).HasPrecision(5, 2);
                entity.Property(e => e.HeroHeadline).HasMaxLength(200);
                entity.Property(e => e.HeroSubheadline).HasMaxLength(500);
                entity.Property(e => e.ValueStatements).HasMaxLength(2000);
                entity.Property(e => e.ProcessSteps).HasMaxLength(2000);
                entity.Property(e => e.InstapayAccount).HasMaxLength(100);
                entity.Property(e => e.VodafoneCashNumber).HasMaxLength(50);
                entity.Property(e => e.CairoGizaShippingCost).HasPrecision(18, 2);
            });

            // ========== COUPON ==========
            builder.Entity<Coupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            });

            // ========== ANNOUNCEMENT ==========
            builder.Entity<Announcement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Style).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
            });
        }
    }
}
