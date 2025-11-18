using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Constants;
using ProductService.Domain.Entities;
using UserService.Domain.Entities;

namespace ProductService.Infrastructure;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Title).HasMaxLength(200);

            b.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId);

            b.HasMany(p => p.Images)
             .WithOne(i => i.Product)
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Seller)
             .WithMany(u => u.Products)
             .HasForeignKey(p => p.SellerId);

            b.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
            b.Property(p => p.Price).HasColumnType(DbConstants.MoneyType);

            b.HasIndex(p => p.SellerId);
        });

        modelBuilder.Entity<Category>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<ProductImage>(b =>
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.Url).IsRequired();
        });

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);

            b.Property(u => u.Username).IsRequired().HasMaxLength(100);
            b.HasIndex(u => u.Username).IsUnique();

            b.Property(u => u.Email).IsRequired().HasMaxLength(250);
            b.HasIndex(u => u.Email).IsUnique();

            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);

            b.HasOne(u => u.Profile)
             .WithOne(up => up.User)
             .HasForeignKey<UserProfile>(up => up.UserId);
        });

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(up => up.UserId);

            b.Property(up => up.AvatarUrl).HasMaxLength(500);
            b.Property(up => up.Bio).HasMaxLength(1000);
            b.Property(up => up.RatingScore).HasColumnType("float");
        });
    }
}
