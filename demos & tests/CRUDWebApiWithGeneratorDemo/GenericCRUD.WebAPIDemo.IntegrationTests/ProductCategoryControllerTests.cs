using CRUDWebApiWithGeneratorDemo;
using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Database.DbContexts;
using DotNetBrightener.TestHelpers;
using GenericCRUD.WebAPIDemo.IntegrationTests.StartupTasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Json;
using Shouldly;

namespace GenericCRUD.WebAPIDemo.IntegrationTests;

public class ProductCategoryControllerTestFactory : MsSqlWebApiTestFactory<CRUDWebApiGeneratorRegistration, MainAppDbContext>
{
    protected override void ConfigureTestServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<ProductDataSeeding>();
    }
}

public class ProductCategoryControllerTests(ProductCategoryControllerTestFactory apiFactory)
    : IClassFixture<ProductCategoryControllerTestFactory>
{
    private readonly HttpClient _client = apiFactory.CreateClient();

    [Fact]
    public async Task ProductCategory_GetLists_ShouldReturnData_WithoutProducts()
    {
        var response = await _client.GetAsync("/api/ProductCategory?columns=name");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var productCategories = await response.Content.ReadFromJsonAsync<List<ProductCategory>>();

        productCategories.ShouldNotBeNull();

        productCategories!.Count
                         .ShouldBe(ProductDataSeeding.CategoryNames.Length);

        var products = productCategories.Where(c => c.Products is not null)
                                        .SelectMany(c => c.Products)
                                        .ToList();

        products.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ProductCategory_GetLists_ShouldReturnData_WithProducts()
    {
        var response = await _client.GetAsync("/api/ProductCategory?columns=name,products");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var productCategories = await response.Content.ReadFromJsonAsync<List<ProductCategory>>();

        productCategories.ShouldNotBeNull();

        productCategories!.Count.ShouldBe(ProductDataSeeding.CategoryNames.Length);

        var products = productCategories.SelectMany(c => c.Products).ToList();

        products.Count().ShouldBeGreaterThan(0);

        //response.Headers.Location!.ToString().Should()
        //        .Be($"http://localhost/customers/{customerResponse!.Id}");
    }

    [Fact]
    public async Task ProductCategory_GetLists_Filter_ShouldReturnData_WithProducts()
    {
        var response = await _client.GetAsync("/api/ProductCategory?columns=name,createdBy,createdDate,products.name,products.description&name=sw(shoes)");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();

        var jobject = JArray.Parse(responseString);

        jobject.Count.ShouldBe(1);

        // validate the JSON to be return with only the requested columns
        foreach (var item in jobject)
        {
            var itemDictionary = item.ToObject<Dictionary<string, object>>();

            itemDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdDate", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "createdBy", "The query requests for it");
            itemDictionary.ShouldContain(c => c.Key == "products", "The query requests for it");

            itemDictionary.ShouldNotContain(c => c.Key == "modifiedDate", "The query doesn't request for it");
            itemDictionary.ShouldNotContain(c => c.Key == "modifiedBy", "The query doesn't request for it");

            foreach (var product in item["products"])
            {
                var productDictionary = product.ToObject<Dictionary<string, object>>();
                
                productDictionary.ShouldContain(c => c.Key == "name", "The query requests for it");
                productDictionary.ShouldContain(c => c.Key == "description", "The query requests for it");
                productDictionary.ShouldNotContain(c => c.Key == "createdDate", "The query doesn't request for it");
                productDictionary.ShouldNotContain(c => c.Key == "documents", "The query doesn't request for it");
            }
        }

        var productCategories = await response.Content.ReadFromJsonAsync<List<ProductCategory>>();

        productCategories.ShouldNotBeNull();

        productCategories!.Count.ShouldBe(1);

        var products = productCategories.SelectMany(c => c.Products).ToList();

        products.Count().ShouldBeGreaterThan(0);
        products.Count().ShouldBe(3);
    }
}