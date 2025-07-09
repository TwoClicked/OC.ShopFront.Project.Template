using Microsoft.EntityFrameworkCore;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ObjectLayer.Chat;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ObjectLayer.Orders;

namespace OC.LUAC.DataLayer
{
    /// <summary>
    /// Represents the application database context for Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class with the specified options.
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


        // Accounts

        public DbSet<Customer> Customers { get; set; } // Represents the Customers table in the database
        public DbSet<AdminUser> AdminUsers { get; set; } // Represents the AdminUsers table in the database
        public DbSet<Address> Addresses { get; set; } // Represents the Addresses table in the database

        // Products

        public DbSet<Category> Categories { get; set; } // Represents the Categories table in the database
        public DbSet<Product> Products { get; set; } // Represents the Products table in the database
        public DbSet<ProductImage> ProductImages { get; set; } // Represents the ProductImages table in the database
        public DbSet<ProductVariant> ProductVariants { get; set; } // Represents the ProductVariants table in the database
        public DbSet<StockAction> StockActions { get; set; } // Represents the StockActions table in the database

        // Orders

        public DbSet<Order> Orders { get; set; } // Represents the Orders table in the database
        public DbSet<OrderItem> OrderItems { get; set; } // Represents the OrderItems table in the database

        // Chat

        public DbSet<ChatSession> ChatSessions { get; set; } // Represents the ChatSessions table in the database
        public DbSet<ChatMessage> ChatMessages { get; set; } // Represents the ChatMessages table in the database


        /// <summary>
        /// Configures the model for the application database context.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //ENUMs stored as strings
            modelBuilder.Entity<StockAction>()
                .Property(sa => sa.ActionType)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // Product > Category (Restict)

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


            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); // 18 digits total, 2 decimal places

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
        }
    }
}
