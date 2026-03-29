using Bogus;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.Services;

public class SeedDataService
{
    public IReadOnlyList<User> Users { get; }
    public IReadOnlyList<Company> Companies { get; }
    public IReadOnlyList<Product> Products { get; }
    public IReadOnlyList<Order> Orders { get; }
    public IReadOnlyList<BlogPost> BlogPosts { get; }

    public SeedDataService()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.IndexGlobal + 1)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
            .RuleFor(u => u.DateOfBirth, f => f.Date.Past(30))
            .RuleFor(u => u.IsActive, f => f.Random.Bool())
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
            .RuleFor(u => u.LastLoginAt, f => f.Date.Recent(30).OrNull(f))
            .RuleFor(u => u.Role, f => f.PickRandom<UserRole>())
            .RuleFor(u => u.Profile, f => new UserProfile
            {
                Bio = f.Lorem.Paragraph(),
                AvatarUrl = f.Internet.Avatar(),
                Website = f.Internet.Url(),
                SocialLinks = f.Make(f.Random.Number(0, 3), () => new SocialLink
                {
                    Platform = f.PickRandom("Twitter", "LinkedIn", "GitHub", "Facebook"),
                    Url = f.Internet.Url()
                }).ToList()
            });

        Users = userFaker.Generate(20);

        var companyFaker = new Faker<Company>()
            .RuleFor(c => c.Id, f => f.IndexGlobal + 1)
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Industry, f => f.Company.CompanySuffix())
            .RuleFor(c => c.FoundedDate, f => f.Date.Past(20))
            .RuleFor(c => c.EmployeeCount, f => f.Random.Number(10, 1000))
            .RuleFor(c => c.Revenue, f => f.Random.Decimal(100000, 100000000))
            .RuleFor(c => c.Address, f => new Address
            {
                Street = f.Address.StreetAddress(),
                City = f.Address.City(),
                State = f.Address.State(),
                ZipCode = f.Address.ZipCode(),
                Country = f.Address.Country()
            });

        Companies = companyFaker.Generate(5);

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexGlobal + 1)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 1000)))
            .RuleFor(p => p.StockQuantity, f => f.Random.Number(0, 500))
            .RuleFor(p => p.Category, f => f.Commerce.Department())
            .RuleFor(p => p.Tags, f => f.Commerce.Categories(3).ToList())
            .RuleFor(p => p.IsFeatured, f => f.Random.Bool())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent(30).OrNull(f))
            .RuleFor(p => p.InternalSku, f => f.Commerce.Ean13());

        Products = productFaker.Generate(30);

        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexGlobal + 1)
            .RuleFor(o => o.OrderDate, f => f.Date.Past(1))
            .RuleFor(o => o.TotalAmount, f => decimal.Parse(f.Commerce.Price(50, 2000)))
            .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
            .RuleFor(o => o.Customer, f => Users[f.Random.Number(0, Users.Count - 1)])
            .RuleFor(o => o.ShippingAddress, f => new Address
            {
                Street = f.Address.StreetAddress(),
                City = f.Address.City(),
                State = f.Address.State(),
                ZipCode = f.Address.ZipCode(),
                Country = f.Address.Country()
            })
            .RuleFor(o => o.Items, f => f.Make(f.Random.Number(1, 5), () => new OrderItem
            {
                ProductId = Products[f.Random.Number(0, Products.Count - 1)].Id,
                ProductName = f.Commerce.ProductName(),
                Quantity = f.Random.Number(1, 10),
                UnitPrice = decimal.Parse(f.Commerce.Price(10, 500)),
                LineTotal = 0
            }).ToList());

        Orders = orderFaker.Generate(15);

        foreach (var order in Orders)
        {
            foreach (var item in order.Items)
            {
                item.LineTotal = item.Quantity * item.UnitPrice;
            }
        }

        var blogPostFaker = new Faker<BlogPost>()
            .RuleFor(b => b.Id, f => f.IndexGlobal + 1)
            .RuleFor(b => b.Title, f => f.Lorem.Sentence())
            .RuleFor(b => b.Content, f => f.Lorem.Paragraphs(3))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.CreatedAt, f => f.Date.Past(1))
            .RuleFor(b => b.UpdatedAt, f => f.Date.Recent(30).OrNull(f))
            .RuleFor(b => b.IsPublished, f => f.Random.Bool())
            .RuleFor(b => b.ViewCount, f => f.Random.Number(0, 10000))
            .RuleFor(b => b.Tags, f => f.Lorem.Words(3).ToList());

        BlogPosts = blogPostFaker.Generate(10);
    }
}
