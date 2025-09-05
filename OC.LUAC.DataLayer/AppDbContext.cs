using Microsoft.EntityFrameworkCore;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ObjectLayer.Chat;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ObjectLayer.Orders;

namespace OC.LUAC.DataLayer
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Accounts
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        // Products
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<StockAction> StockActions { get; set; }

        // Orders
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ShippingZone> ShippingZones { get; set; }
        public DbSet<ShippingZoneCountry> ShippingZoneCountries { get; set; }

        // Chat
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ENUMs stored as strings
            modelBuilder.Entity<StockAction>()
                .Property(sa => sa.ActionType)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // Product > Category (Restrict)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → Images (Cascade)
            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → Variants (Cascade)
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Variant → StockActions (Cascade)
            modelBuilder.Entity<StockAction>()
                .HasOne(sa => sa.ProductVariant)
                .WithMany(v => v.StockActions)
                .HasForeignKey(sa => sa.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order → StockActions (Restrict)
            modelBuilder.Entity<StockAction>()
                .HasOne(sa => sa.Order)
                .WithMany()
                .HasForeignKey(sa => sa.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → OrderItems (Cascade)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem → Product (Restrict) 
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem → ProductVariant (Restrict) 
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer → Orders (Restrict)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer → Addresses (Cascade)
            modelBuilder.Entity<Address>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatSession → ChatMessages (Cascade)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.ChatSession)
                .WithMany(cs => cs.Messages)
                .HasForeignKey(cm => cm.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatSession → Customer (Restrict)
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Customer)
                .WithMany()
                .HasForeignKey(cs => cs.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Precision rules ----------
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalBeforeDiscount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAfterDiscount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            // NEW: shipping/totals missing precision
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShippingZone>()
                .Property(z => z.BaseCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShippingZone>()
                .Property(z => z.FreeShippingThreshold)
                .HasPrecision(18, 2);

            // (Optional) voucher precision to silence warnings
            modelBuilder.Entity<Voucher>()
                .Property(v => v.FixedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Voucher>()
                .Property(v => v.Percentage)
                .HasPrecision(5, 2);

            // ---------- Relationships ----------
            modelBuilder.Entity<ShippingZoneCountry>()
                .HasOne(c => c.ShippingZone)
                .WithMany(z => z.Countries)
                .HasForeignKey(c => c.ShippingZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // NEW: prevent duplicate country in the same zone
            modelBuilder.Entity<ShippingZoneCountry>()
                .HasIndex(x => new { x.ShippingZoneId, x.CountryCode })
                .IsUnique();

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            modelBuilder.Entity<ShippingZoneCountry>()
                .Property(c => c.CountryCode)
                .HasMaxLength(100);
            modelBuilder.Entity<ShippingZoneCountry>()
                .Property(c => c.CountryName)
                .HasMaxLength(100);

            // Global filters for soft-delete
            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<ProductImage>()
                .HasQueryFilter(i => !i.IsDeleted);

            modelBuilder.Entity<ProductVariant>()
                .HasQueryFilter(v => !v.IsDeleted);
        }
    }
}
