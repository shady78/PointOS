using Microsoft.EntityFrameworkCore;
using PointOS.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PointOS.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Your connection string config if needed
        }

        // Suppress the specific warning about pending model changes
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<Order>()
        .HasMany(o => o.OrderItems)
        .WithOne(oi => oi.Order)
        .HasForeignKey(oi => oi.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

        // Seed with static, hardcoded dates
        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                Id = 1,
                OrderNumber = "13336",
                CustomerName = "Brian Cox",
                Total = 127.00M,
                Status = "Completed",
                DateAdded = new DateTime(2023, 5, 1), // Static date
                DateModified = new DateTime(2023, 5, 6), // Static date
                CustomerInitials = "BC",
                CustomerInitialsBackgroundColor = "#FF6B6B"
            },
            new Order
            {
                Id = 2,
                OrderNumber = "13337",
                CustomerName = "Robert Doe",
                Total = 34.00M,
                Status = "Delivering",
                DateAdded = new DateTime(2023, 4, 30), // Static date
                DateModified = new DateTime(2023, 5, 7), // Static date
                CustomerInitials = "RD",
                CustomerInitialsBackgroundColor = "#4ECDC4"
            }
        // Add more sample orders as needed
        );


        // Seed some initial categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Computers", Description = "Our computers and tablets include all the big brands.", Type = "Automated", Image = "/media/categories/computers.png" },
            new Category { Id = 2, Name = "Watches", Description = "Our range of watches are perfect whether you're looking to upgrade", Type = "Automated", Image = "/media/categories/watches.png" },
            new Category { Id = 3, Name = "Headphones", Description = "Our big range of headphones makes it easy to upgrade your device at a great price.", Type = "Manual", Image = "/media/categories/headphones.png" },
            new Category { Id = 4, Name = "Footwear", Description = "Whatever your activity needs are, we've got you covered.", Type = "Automated", Image = "/media/categories/footwear.png" }
        );

        // Seed some products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Product 1", SKU = "02743005", Price = 229.00M, Quantity = 50, CategoryId = 1, Status = "Scheduled", Rating = 3 },
            new Product { Id = 2, Name = "Product 2", SKU = "03961008", Price = 159.00M, Quantity = 44, CategoryId = 2, Status = "Inactive", Rating = 4 },
            new Product { Id = 3, Name = "Product 3", SKU = "03819003", Price = 21.00M, Quantity = 30, CategoryId = 3, Status = "Inactive", Rating = 4 },
            new Product { Id = 4, Name = "Product 4", SKU = "03605007", Price = 159.00M, Quantity = 43, CategoryId = 4, Status = "Published", Rating = 4 }
        );
    }
}