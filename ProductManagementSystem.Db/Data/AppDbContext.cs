using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.Db.Entities;

namespace ProductManagementSystem.Db.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. ตั้งค่าความสัมพันธ์ (Category - Product Relationship)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // ห้ามลบ Category ถ้ายังมี Product ในนั้นอยู่

        // 2. ตั้งค่า Constraints (ห้ามข้อมูลซ้ำในระดับฐานข้อมูล)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // 3. ใส่ข้อมูลเริ่มต้น (Seed Data)
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // รหัสผ่านคือ admin123 (เข้ารหัสล่วงหน้า)
        var adminPasswordHash = "340AACE49FB2DFEBDF5B70CF505213FA:F98DCFC2E00EB1925BC73FCF3AB41889BA66E44CBF317ECDAB2FE618F68C506E";
        // รหัสผ่านคือ user123 (เข้ารหัสล่วงหน้า)
        var userPasswordHash = "AFF486CE5049A27398FB89FCA93F3028:539FAE1CE7948801FFD24A8F3CCA877279AB965265AA0A5288034CB1A0474890";

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = adminPasswordHash,
                FullName = "System Administrator",
                Role = "Admin"
            },
            new User
            {
                Id = 2,
                Username = "user",
                PasswordHash = userPasswordHash,
                FullName = "Standard User",
                Role = "User"
            }
        );

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Smartphones, Laptops, and Accessories" },
            new Category { Id = 2, Name = "Home Appliances", Description = "Kitchenware, TVs, and Home electronics" },
            new Category { Id = 3, Name = "Fashion", Description = "Apparel, Shoes, and Accessories" }
        );

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "iPhone 15 Pro",
                Description = "Apple iPhone 15 Pro 128GB, Natural Titanium",
                Price = 35000.00m,
                Stock = 12,
                CategoryId = 1,
                ImageUrl = "/images/iphone15.jpg"
            },
            new Product
            {
                Id = 2,
                Name = "MacBook Air M3",
                Description = "Apple MacBook Air 13-inch M3, 8GB RAM, 256GB SSD",
                Price = 42000.00m,
                Stock = 8,
                CategoryId = 1,
                ImageUrl = "/images/macbook.jpg"
            },
            new Product
            {
                Id = 3,
                Name = "Smart TV 55\"",
                Description = "Samsung 55-inch Crystal UHD 4K Smart TV",
                Price = 15900.00m,
                Stock = 15,
                CategoryId = 2,
                ImageUrl = "/images/tv.jpg"
            },
            new Product
            {
                Id = 4,
                Name = "Coffee Maker",
                Description = "Automatic Espresso Machine with Milk Frother",
                Price = 4500.00m,
                Stock = 20,
                CategoryId = 2,
                ImageUrl = "/images/coffeemaker.jpg"
            },
            new Product
            {
                Id = 5,
                Name = "Running Shoes",
                Description = "Lightweight breathable sports shoes for men and women",
                Price = 2200.00m,
                Stock = 45,
                CategoryId = 3,
                ImageUrl = "/images/shoes.jpg"
            },
            new Product
            {
                Id = 6,
                Name = "iPad Air M2",
                Description = "Apple iPad Air 11-inch M2, 128GB Wi-Fi, Space Gray",
                Price = 23900.00m,
                Stock = 15,
                CategoryId = 1,
                ImageUrl = "/images/ipad.jpg"
            },
            new Product
            {
                Id = 7,
                Name = "Bluetooth Speaker",
                Description = "Portable waterproof wireless speaker with deep bass",
                Price = 1890.00m,
                Stock = 30,
                CategoryId = 1,
                ImageUrl = "/images/speaker.jpg"
            },
            new Product
            {
                Id = 8,
                Name = "Mechanical Keyboard",
                Description = "RGB backlit hot-swappable mechanical gaming keyboard",
                Price = 3200.00m,
                Stock = 25,
                CategoryId = 1,
                ImageUrl = "/images/keyboard.jpg"
            },
            new Product
            {
                Id = 9,
                Name = "Wireless Gaming Mouse",
                Description = "Lightweight ergonomic rechargeable gaming mouse",
                Price = 2490.00m,
                Stock = 40,
                CategoryId = 1,
                ImageUrl = "/images/mouse.jpg"
            },
            new Product
            {
                Id = 10,
                Name = "Air Fryer",
                Description = "Digital hot air fryer 4.5L with preset cooking modes",
                Price = 3590.00m,
                Stock = 18,
                CategoryId = 2,
                ImageUrl = "/images/airfryer.jpg"
            },
            new Product
            {
                Id = 11,
                Name = "Electric Kettle",
                Description = "Double-wall stainless steel rapid boil electric kettle 1.7L",
                Price = 1290.00m,
                Stock = 50,
                CategoryId = 2,
                ImageUrl = "/images/kettle.jpg"
            },
            new Product
            {
                Id = 12,
                Name = "Personal Blender",
                Description = "High-speed mini bullet blender for smoothies and shakes",
                Price = 1590.00m,
                Stock = 22,
                CategoryId = 2,
                ImageUrl = "/images/blender.jpg"
            },
            new Product
            {
                Id = 13,
                Name = "Yoga Mat",
                Description = "Non-slip eco-friendly exercise mat with carrying strap",
                Price = 690.00m,
                Stock = 60,
                CategoryId = 3,
                ImageUrl = "/images/yogamat.jpg"
            },
            new Product
            {
                Id = 14,
                Name = "Dumbbell Set 10kg",
                Description = "Adjustable chrome dumbbell set with storage case",
                Price = 1490.00m,
                Stock = 35,
                CategoryId = 3,
                ImageUrl = "/images/dumbbell.jpg"
            },
            new Product
            {
                Id = 15,
                Name = "Camping Tent",
                Description = "Waterproof double-layer family camping tent for 3-4 persons",
                Price = 2990.00m,
                Stock = 10,
                CategoryId = 3,
                ImageUrl = "/images/tent.jpg"
            }
        );
    }
}
