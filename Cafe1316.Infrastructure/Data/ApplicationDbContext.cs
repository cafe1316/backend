using Microsoft.EntityFrameworkCore;
using Cafe1316.Domain.Entities;
namespace Cafe1316.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet - 每个实体一个
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Subcategory> Subcategories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<ProductFlavorNote> ProductFlavorNotes { get; set; } = null!;
    public DbSet<ProductTagMapping> ProductTagMappings { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
    public DbSet<CheckoutIntent> CheckoutIntents { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 在这里配置实体关系
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(entity=> entity.Id);
            entity.Property(entity => entity.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(entity => entity.Password)
                .HasMaxLength(255);
            entity.Property(entity => entity.FirstName)
                .HasMaxLength(100);
            entity.Property(entity => entity.LastName)
                .HasMaxLength(100);
            entity.Property(entity => entity.DisplayName)
                .HasMaxLength(120);
            entity.HasIndex(entity => entity.Email).IsUnique();
            entity.HasIndex(entity => entity.DeletedAt); // ← 添加软删除索引
            // 软删除过滤器（重要！）
            entity.HasQueryFilter(e => e.DeletedAt == null);  // ← 自动过滤已删除用户
            
            //关系配置
            // User (1) → UserProfile (1)
            entity.HasOne(entity => entity.Profile)
                .WithOne(entity => entity.User)
                .HasForeignKey<UserProfile>(entity => entity.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (1) → Accounts (Many)
            entity.HasMany(entity => entity.Accounts)
                .WithOne(entity => entity.User)
                .HasForeignKey(entity => entity.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (1) → Addresses (Many)
            entity.HasMany(entity => entity.Addresses)
                .WithOne(entity => entity.User)
                .HasForeignKey(entity => entity.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (1) → CartItems (Many)
            entity.HasMany(entity => entity.CartItems)
                .WithOne(entity => entity.User)
                .HasForeignKey(entity => entity.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (1) → WishlistItems (Many)
            entity.HasMany(entity => entity.WishlistItems)
                .WithOne(entity => entity.User)
                .HasForeignKey(entity => entity.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (1) → CheckoutIntents (Many) ← 添加这个！
            entity.HasMany(e => e.CheckoutIntents)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // User (1) → Orders (Many)
            entity.HasMany(entity => entity.Orders)
                .WithOne(entity => entity.User)
                .HasForeignKey(entity => entity.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.UserId);
            entity.Property(entity => entity.Provider).IsRequired().HasConversion<string>();
            entity.Property(entity => entity.ProviderAccountId).HasMaxLength(255).IsRequired();
            entity.Property(entity => entity.EmailAtLink).HasMaxLength(255);
            entity.HasIndex(entity => new{entity.Provider, entity.UserId}).IsUnique();
            entity.HasIndex(entity => new{entity.Provider, entity.ProviderAccountId}).IsUnique();
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.UserId);
            entity.Property(entity => entity.Nickname).HasMaxLength(50);
            entity.Property(entity => entity.AvatarUrl);
            entity.Property(entity => entity.Gender).HasMaxLength(10);
            entity.Property(entity => entity.BirthDate);
            entity.Property(entity => entity.Phone).HasMaxLength(20);
            entity.Property(entity => entity.Bio);
            entity.HasIndex(entity => entity.UserId).IsUnique();
            
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("addresses");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.UserId);
            entity.Property(entity => entity.Type).IsRequired().HasConversion<string>();
            entity.Property(entity => entity.IsDefault).IsRequired();
            entity.Property(entity => entity.RecipientName).IsRequired().HasMaxLength(120);
            entity.Property(entity => entity.Phone).IsRequired().HasMaxLength(20);
            entity.Property(entity => entity.Province).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.City).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.District).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.AddressText).IsRequired();
            entity.Property(entity => entity.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(entity => entity.CountryCode).IsRequired().HasMaxLength(2);
            entity.HasIndex(entity => entity.UserId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.Slug).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.Name).IsRequired().HasMaxLength(100);
            entity.Property(entity => entity.Description);
            entity.Property(entity => entity.ImageUrl);
            entity.Property(entity => entity.DisplayOrder).IsRequired();
            entity.Property(entity => entity.IsActive).IsRequired();
            entity.HasIndex(entity => entity.Slug).IsUnique();

            //关系
            entity.HasMany(entity => entity.Subcategories)
                .WithOne(entity => entity.Category)
                .HasForeignKey(entity => entity.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(entity => entity.Products)
                .WithOne(entity => entity.Category)
                .HasForeignKey(entity => entity.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.ToTable("subcategories");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.CategoryId);
            entity.Property(entity => entity.Slug).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.Name).IsRequired().HasMaxLength(100);
            entity.Property(entity => entity.Description);
            entity.Property(entity => entity.DisplayOrder).IsRequired();
            entity.Property(entity => entity.IsActive).IsRequired();
            entity.HasIndex(entity => entity.Slug).IsUnique();
            entity.HasIndex(entity => entity.CategoryId);

            //关系
            entity.HasMany(entity => entity.Products)
                .WithOne(entity => entity.Subcategory)
                .HasForeignKey(entity => entity.SubcategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.Uuid).IsRequired();
            entity.Property(entity => entity.Sku).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.CategoryId);
            entity.Property(entity => entity.SubcategoryId);
            entity.Property(entity => entity.Name).IsRequired().HasMaxLength(255);
            entity.Property(entity => entity.Slug).IsRequired().HasMaxLength(255);
            entity.Property(entity => entity.Description);
            entity.Property(entity => entity.PriceCents).IsRequired();
            entity.Property(entity => entity.Currency).IsRequired().HasMaxLength(10);
            entity.Property(entity => entity.Stock).IsRequired();
            entity.Property(entity => entity.Unit).IsRequired().HasMaxLength(20);
            entity.Property(entity => entity.Brand).HasMaxLength(100);
            entity.Property(entity => entity.Weight);
            entity.Property(entity => entity.Origin).HasConversion<string>();
            entity.Property(entity => entity.RoastLevel).HasConversion<string>();
            entity.Property(entity => entity.ProcessingMethod).HasConversion<string>();
            entity.Property(entity => entity.Altitude);
            entity.Property(entity => entity.Varietals).HasMaxLength(255);
            entity.Property(entity => entity.HarvestYear);
            entity.Property(entity => entity.CuppingScore);
            entity.Property(entity => entity.Material).HasMaxLength(100);
            entity.Property(entity => entity.Color).HasMaxLength(50);
            entity.Property(entity => entity.Size).HasMaxLength(50);
            entity.Property(entity => entity.Capacity);
            entity.Property(entity => entity.Specifications);
            entity.Property(entity => entity.IsFeatured).IsRequired();
            entity.Property(entity => entity.IsActive).IsRequired();
            entity.HasIndex(entity => entity.Uuid).IsUnique();
            entity.HasIndex(entity => entity.Sku).IsUnique();
            entity.HasIndex(entity => entity.Slug).IsUnique();
            entity.HasIndex(entity => entity.CategoryId);
            entity.HasIndex(entity => entity.SubcategoryId);
            entity.HasIndex(entity => entity.IsActive);
            entity.HasIndex(entity => entity.IsFeatured);
            entity.HasIndex(entity => entity.Origin);
            entity.HasIndex(entity => entity.RoastLevel);

            //关系
            entity.HasMany(entity => entity.Images)
                .WithOne(entity => entity.Product)
                .HasForeignKey(entity => entity.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(entity => entity.FlavorNotes)
                .WithOne(entity => entity.Product)
                .HasForeignKey(entity => entity.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(entity => entity.Tags)
                .WithOne(entity => entity.Product)
                .HasForeignKey(entity => entity.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(entity => entity.CartItems)
                .WithOne(entity => entity.Product)
                .HasForeignKey(entity => entity.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(entity => entity.WishlistItems)
                .WithOne(entity => entity.Product)
                .HasForeignKey(entity => entity.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(entity => entity.OrderItems)
                .WithOne(entity => entity.Product)
                .HasForeignKey(entity => entity.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductFlavorNote>(entity =>
        {
            entity.ToTable("product_flavor_notes");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.ProductId);
            entity.Property(entity => entity.FlavorNote).IsRequired().HasConversion<string>();
            entity.HasIndex(entity => new{entity.ProductId, entity.FlavorNote}).IsUnique();
        });

        modelBuilder.Entity<ProductTagMapping>(entity =>
        {
            entity.ToTable("product_tags");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.ProductId);
            entity.Property(entity => entity.Tag).IsRequired().HasConversion<string>();
            entity.HasIndex(entity => new{entity.ProductId, entity.Tag}).IsUnique();
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.ProductId);
            entity.Property(entity => entity.ImageUrl).IsRequired();
            entity.Property(entity => entity.IsPrimary).IsRequired();
            entity.Property(entity => entity.DisplayOrder).IsRequired();
            entity.HasIndex(entity => entity.ProductId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items", table =>
                table.HasCheckConstraint(
                    "CK_cart_items_Quantity_Range",
                    "\"Quantity\" >= 1 AND \"Quantity\" <= 99"));
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.UserId); 
            entity.Property(entity => entity.ProductId);
            entity.Property(entity => entity.Quantity).IsRequired();
            entity.HasIndex(entity => new{entity.UserId, entity.ProductId}).IsUnique();
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("wishlist_items");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.UserId); 
            entity.Property(entity => entity.ProductId);
            entity.HasIndex(entity => new{entity.UserId, entity.ProductId}).IsUnique();
        });

        modelBuilder.Entity<CheckoutIntent>(entity =>
        {
            entity.ToTable("checkout_intents");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.Uuid).IsRequired();
            entity.Property(entity => entity.UserId);
            entity.Property(entity => entity.SelectedItems).IsRequired().HasColumnType("jsonb");
            entity.Property(entity => entity.ShippingAddress).HasColumnType("jsonb");
            entity.Property(entity => entity.BillingAddress).HasColumnType("jsonb");
            entity.Property(entity => entity.ShippingMethod).HasMaxLength(50);
            entity.Property(entity => entity.SubtotalCents).IsRequired();
            entity.Property(entity => entity.ShippingFeeCents).IsRequired();
            entity.Property(entity => entity.TaxCents).IsRequired();
            entity.Property(entity => entity.GrandTotalCents).IsRequired();
            entity.Property(entity => entity.Currency).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.CompletedOrderId);
            entity.Property(entity => entity.ExpiresAt).IsRequired();
            entity.HasIndex(entity => entity.Uuid).IsUnique();
            entity.HasIndex(entity => entity.UserId);
            entity.HasIndex(entity => entity.ExpiresAt);

            //关系
            // CheckoutIntent (1) ↔ Order (1) - 可选
            entity.HasOne(e => e.CompletedOrder)
                .WithOne()  // Order 没有 CheckoutIntent 导航属性
                .HasForeignKey<CheckoutIntent>(e => e.CompletedOrderId)  // 外键在 CheckoutIntent
                .OnDelete(DeleteBehavior.SetNull);  // 删除 Order 时设为 null

        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.Uuid).IsRequired();
            entity.Property(entity => entity.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.UserId).IsRequired();
            entity.Property(entity => entity.Email).IsRequired().HasMaxLength(255);
            entity.Property(entity => entity.ShippingAddress).IsRequired().HasColumnType("jsonb");
            entity.Property(entity => entity.BillingAddress).HasColumnType("jsonb");
            entity.Property(entity => entity.SubtotalCents).IsRequired();
            entity.Property(entity => entity.ShippingFeeCents).IsRequired();
            entity.Property(entity => entity.TaxCents).IsRequired();
            entity.Property(entity => entity.GrandTotalCents).IsRequired();
            entity.Property(entity => entity.Currency).IsRequired().HasMaxLength(10);
            entity.Property(entity => entity.Status).IsRequired().HasConversion<string>();
            entity.Property(entity => entity.StripeSessionId).HasMaxLength(255);
            entity.Property(entity => entity.StripePaymentIntentId).HasMaxLength(255);
            entity.Property(entity => entity.PaidAt);
            entity.Property(entity => entity.ShippedAt);
            entity.Property(entity => entity.DeliveredAt);
            entity.Property(entity => entity.Notes);
            entity.HasIndex(entity => entity.Uuid).IsUnique();
            entity.HasIndex(entity => entity.OrderNumber).IsUnique();
            entity.HasIndex(entity => entity.StripePaymentIntentId).IsUnique();
            entity.HasIndex(entity => entity.Status);
            entity.HasIndex(entity => entity.CreatedAt);
            entity.HasIndex(entity => entity.UserId);

            //关系
            entity.HasMany(entity => entity.OrderItems)
                .WithOne(entity => entity.Order)
                .HasForeignKey(entity => entity.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order (1) ↔ Payment (1)
            entity.HasOne(entity => entity.Payment)
                .WithOne(entity => entity.Order)
                .HasForeignKey<Payment>(entity => entity.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.OrderId);
            entity.Property(entity => entity.ProductId);
            entity.Property(entity => entity.ProductName).IsRequired().HasMaxLength(255);
            entity.Property(entity => entity.ProductSku).IsRequired().HasMaxLength(50);
            entity.Property(entity => entity.ProductSlug).HasMaxLength(255);
            entity.Property(entity => entity.ImageUrl);
            entity.Property(entity => entity.UnitPriceCents).IsRequired();
            entity.Property(entity => entity.Quantity).IsRequired();
            entity.Property(entity => entity.Currency).IsRequired().HasMaxLength(10);
            entity.HasIndex(entity => entity.OrderId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.Uuid).IsRequired();
            entity.Property(entity => entity.OrderId);
            entity.Property(entity => entity.PaymentMethod).IsRequired().HasConversion<string>();
            entity.Property(entity => entity.TransactionId).HasMaxLength(100);
            entity.Property(entity => entity.AmountCents).IsRequired();
            entity.Property(entity => entity.Currency).IsRequired().HasMaxLength(10);
            entity.Property(entity => entity.Status).IsRequired().HasConversion<string>();
            entity.Property(entity => entity.PaidAt);
            entity.HasIndex(entity => entity.Uuid).IsUnique();
            entity.HasIndex(entity => entity.OrderId).IsUnique();
            entity.HasIndex(entity => entity.TransactionId).IsUnique();
            entity.HasIndex(entity => entity.Status);
        });

    }

}
