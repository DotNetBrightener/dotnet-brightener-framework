using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp.CommonShared.Endpoints;
using Xunit;

namespace WebApp.CommonShared.Tests;

/// <summary>
///     Unit tests for EndpointModuleBase
/// </summary>
public class EndpointModuleBaseTests
{
    [Fact]
    public void EndpointModuleBase_WithDefaultValues_ShouldHaveEmptyBasePath()
    {
        // Arrange & Act
        var module = new DefaultTestModule();

        // Assert
        module.ExposedBasePath.ShouldBeEmpty();
    }

    [Fact]
    public void EndpointModuleBase_WithDefaultValues_ShouldHaveEmptyTags()
    {
        // Arrange & Act
        var module = new DefaultTestModule();

        // Assert
        module.ExposedTags.ShouldBeEmpty();
    }

    [Fact]
    public void EndpointModuleBase_WithCustomBasePath_ShouldReturnConfiguredPath()
    {
        // Arrange & Act
        var module = new CustomBasePathModule("/api/users");

        // Assert
        module.ExposedBasePath.ShouldBe("/api/users");
    }

    [Fact]
    public void EndpointModuleBase_WithCustomTags_ShouldReturnConfiguredTags()
    {
        // Arrange & Act
        var module = new CustomTagsModule("Users", "Administration");

        // Assert
        module.ExposedTags.ShouldBe(new[] { "Users", "Administration" });
    }

    [Fact]
    public void Map_ShouldCreateRouteGroupAndCallMapEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var builder = new TestEndpointRouteBuilder(serviceProvider);
        var module = new MapEndpointsTestModule();

        // Act
        module.Map(builder);

        // Assert
        module.MapEndpointsWasCalled.ShouldBeTrue();
    }

    [Fact]
    public void Map_WithConfigureGroup_ShouldInvokeConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var builder = new TestEndpointRouteBuilder(serviceProvider);
        var module = new ConfigureGroupTestModule();

        // Act
        module.Map(builder);

        // Assert
        module.ConfigureGroupWasCalled.ShouldBeTrue();
    }

    #region Test Implementations

    private class DefaultTestModule : TestModuleBase
    {
    }

    private class CustomBasePathModule : TestModuleBase
    {
        private readonly string _basePath;

        public CustomBasePathModule(string basePath)
        {
            _basePath = basePath;
        }

        protected override string BasePath => _basePath;
    }

    private class CustomTagsModule : TestModuleBase
    {
        private readonly string[] _tags;

        public CustomTagsModule(params string[] tags)
        {
            _tags = tags;
        }

        protected override string[] Tags => _tags;
    }

    private class MapEndpointsTestModule : TestModuleBase
    {
        public bool MapEndpointsWasCalled { get; private set; }

        protected override void MapEndpoints(IEndpointRouteBuilder app)
        {
            MapEndpointsWasCalled = true;
        }
    }

    private class ConfigureGroupTestModule : TestModuleBase
    {
        public bool ConfigureGroupWasCalled { get; private set; }

        protected override Action<RouteGroupBuilder>? ConfigureGroup => group =>
        {
            ConfigureGroupWasCalled = true;
        };
    }

    private abstract class TestModuleBase : EndpointModuleBase
    {
        public string ExposedBasePath => BasePath;
        public string[] ExposedTags => Tags;

        protected override void MapEndpoints(IEndpointRouteBuilder app)
        {
            // Override in specific tests
        }
    }

    private class TestEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public TestEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
        public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

        public IApplicationBuilder CreateApplicationBuilder()
        {
            return new ApplicationBuilder(ServiceProvider);
        }
    }

    #endregion
}
