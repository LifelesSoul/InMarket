using Bogus;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace ProductService.Infrastructure;

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

        var users = new List<User>();
        var profiles = new List<UserProfile>();
        var faker = new Faker();

        for (int i = 0; i < 50; i++)
        {
            var userId = Guid.NewGuid();
            var username = faker.Internet.UserName();

            var user = new User
            {
                Id = userId,
                Username = username,
                Email = faker.Internet.Email(username),
                PasswordHash = faker.Internet.Password(),
                Role = faker.PickRandom<UserRole>(),
                RegistrationDate = faker.Date.PastOffset(2),
                Profile = null!
            };

            var profile = new UserProfile
            {
                UserId = userId,
                User = user,
                AvatarUrl = faker.Internet.Avatar(),
                Biography = faker.Lorem.Sentence(),
                RatingScore = faker.Random.Double(0, 5)
            };

            user.Profile = profile;

            users.Add(user);
            profiles.Add(profile);
        }

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
