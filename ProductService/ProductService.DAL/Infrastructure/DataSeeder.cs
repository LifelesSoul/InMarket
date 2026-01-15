using Bogus;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace ProductService.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DataSeeder
{
    public static void SeedData(this ProductDbContext context)
    {
        if (context.Users.Any()) return;

        var categoryNames = new[] { "Electronics", "Books", "Clothing", "Home & Garden", "Toys", "Sports", "Automotive", "Music" };
        var categories = new List<Category>();

        foreach (var name in categoryNames)
        {
            categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = name
            });
        }

        var profiles = new List<UserProfile>();

        var userFaker = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Username))
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
                .RuleFor(u => u.Role, f => f.PickRandom<UserRole>())
                .RuleFor(u => u.RegistrationDate, f => f.Date.PastOffset(2))
                .FinishWith((f, u) =>
                {
                    var profile = new UserProfile
                    {
                        UserId = u.Id,
                        User = u,
                        AvatarUrl = f.Internet.Avatar(),
                        Biography = f.Lorem.Sentence(),
                        RatingScore = f.Random.Double(0, 5)
                    };

                    u.Profile = profile;
                    profiles.Add(profile);
                });

        var users = userFaker.Generate(50);

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price()))
            .RuleFor(p => p.CreationDate, f => f.Date.PastOffset(1))
            .RuleFor(p => p.Priority, f => f.PickRandom<Priority>())
            .RuleFor(p => p.Status, f => f.PickRandom<ProductStatus>())
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(p => p.Category, (f, p) => categories.First(c => c.Id == p.CategoryId))
            .RuleFor(p => p.SellerId, f => f.PickRandom(users).Id)
            .RuleFor(p => p.Seller, (f, p) => users.First(u => u.Id == p.SellerId));

        var products = productFaker.Generate(100);

        var imageFaker = new Faker<ProductImage>()
            .RuleFor(i => i.Id, f => Guid.NewGuid())
            .RuleFor(i => i.Url, f => f.Image.PicsumUrl())
            .RuleFor(i => i.ProductId, f => f.PickRandom(products).Id);

        var images = imageFaker.Generate(300);

        context.Categories.AddRange(categories);
        context.Users.AddRange(users);
        context.Products.AddRange(products);
        context.ProductImages.AddRange(images);

        context.SaveChanges();
    }
}
