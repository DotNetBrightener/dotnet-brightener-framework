using Bogus;
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericCRUD.WebAPIDemo.IntegrationTests.StartupTasks;

internal class ProductDataSeeding(
    IServiceScopeFactory     serviceScopeFactory,
    IHostApplicationLifetime lifetime,
    ILoggerFactory           loggerFactory)
    : DataSeedingStartupTask(serviceScopeFactory, lifetime, loggerFactory)
{
    public static readonly string[] CategoryNames =
    [
        "Shoes", "Clothes", "Electronics", "Furniture", "Books", "Toys", "Tools", "Food", "Drinks", "Health", "Beauty",
        "Sports", "Music", "Movies", "Games", "Pets", "Garden", "Automotive", "Industrial", "Software", "Hardware",
        "Services", "Other"
    ];

    protected override async Task Seed(IServiceProvider serviceProvider)
    {
        var productCategoryService = serviceProvider.GetRequiredService<IProductCategoryDataService>();

        var categoriesList = new List<ProductCategory>();
        var faker          = new Faker();

        foreach (var item in CategoryNames)
        {
            categoriesList.Add(new ProductCategory
            {
                Name = $"{item}",
                Products = new List<Product>
                {
                    new Product
                    {
                        Name        = $"{item} 1 - should have 1 document",
                        Description = faker.Commerce.ProductDescription(),
                        ProductDocuments = new List<ProductDocument>
                        {
                            new ProductDocument
                            {
                                FileName    = $"{item} 1 File 1-" + faker.System.FileName(),
                                Description = faker.Commerce.ProductDescription()
                            },

                        }
                    },
                    new Product
                    {
                        Name        = $"{item} 2 - should have 2 documents",
                        Description = faker.Commerce.ProductDescription(),
                        ProductDocuments = new List<ProductDocument>
                        {
                            new ProductDocument
                            {
                                FileName    = $"{item} 2 File 1-" + faker.System.FileName(),
                                Description = faker.Commerce.ProductDescription()
                            },
                            new ProductDocument
                            {
                                FileName    = $"{item} 2 File 2-" + faker.System.FileName(),
                                Description = faker.Commerce.ProductDescription()
                            },
                        }
                    },
                    new Product
                    {
                        Name        = $"{item} 3 - should have 3 documents",
                        Description = faker.Commerce.ProductDescription(),
                        ProductDocuments = new List<ProductDocument>
                        {
                            new ProductDocument
                            {
                                FileName    = $"{item} 3 File 1-" + faker.System.FileName(),
                                Description = faker.Commerce.ProductDescription()
                            },
                            new ProductDocument
                            {
                                FileName    = $"{item} 3 File 2-" + faker.System.FileName(),
                                Description = faker.Commerce.ProductDescription()
                            },
                            new ProductDocument
                            {
                                FileName    = $"{item} 3 File 3-" + faker.System.FileName(),
                                Description = faker.Commerce.ProductDescription()
                            },
                        }
                    },
                }
            });
        }

        await productCategoryService.InsertManyAsync(categoriesList);
    }
}