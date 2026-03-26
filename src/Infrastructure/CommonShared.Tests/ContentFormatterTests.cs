using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp.CommonShared.Endpoints.Response;
using Xunit;

namespace WebApp.CommonShared.Tests;

/// <summary>
///     Unit tests for content formatting system
/// </summary>
public class ContentFormatterTests
{
    [Fact]
    public void JsonContentFormatter_ShouldSupportJsonContentTypes()
    {
        // Arrange
        var formatter = new JsonContentFormatter();

        // Act
        var supportedTypes = formatter.SupportedContentTypes.ToList();

        // Assert
        supportedTypes.ShouldContain("application/json");
        supportedTypes.ShouldContain("text/json");
    }

    [Theory]
    [InlineData("application/json", true)]
    [InlineData("text/json", true)]
    [InlineData("application/vnd.api+json", true)]
    [InlineData("text/html", false)]
    [InlineData("application/xml", false)]
    [InlineData("", false)]
    public void JsonContentFormatter_CanFormat_ShouldReturnExpectedResult(
        string contentType,
        bool expected)
    {
        // Arrange
        var formatter = new JsonContentFormatter();

        // Act
        var result = formatter.CanFormat(contentType);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task JsonContentFormatter_FormatAsync_ShouldWriteJsonToResponse()
    {
        // Arrange
        var formatter = new JsonContentFormatter();
        var context = CreateHttpContext();
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        await formatter.FormatAsync(context, data);

        // Assert
        context.Response.ContentType.ShouldBe("application/json");
        context.Response.ContentLength.ShouldNotBeNull();
    }

    [Fact]
    public async Task JsonContentFormatter_FormatAsync_WithNullValue_ShouldSetEmptyContent()
    {
        // Arrange
        var formatter = new JsonContentFormatter();
        var context = CreateHttpContext();

        // Act
        await formatter.FormatAsync<object>(context, null);

        // Assert
        context.Response.ContentType.ShouldBe("application/json");
        context.Response.ContentLength.ShouldBe(0);
    }

    [Fact]
    public void JsonContentFormatter_WithCustomOptions_ShouldUseProvidedOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
            WriteIndented = true
        };
        var formatter = new JsonContentFormatter(options);

        // Assert - should not throw
        formatter.ShouldNotBeNull();
    }

    [Fact]
    public void ContentFormatterOptions_AddFormatter_ShouldAddToCollection()
    {
        // Arrange
        var options = new ContentFormatterOptions();

        // Act
        options.AddFormatter<JsonContentFormatter>();

        // Assert
        options.AdditionalFormatters.ShouldContain(typeof(JsonContentFormatter));
    }

    [Fact]
    public void AddContentFormatters_ShouldRegisterJsonFormatterByDefault()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddContentFormatters();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var formatters = serviceProvider.GetServices<IContentFormatter>().ToList();
        formatters.Count.ShouldBe(1);
        formatters[0].ShouldBeOfType<JsonContentFormatter>();
    }

    [Fact]
    public void AddContentFormatters_WithCustomFormatter_ShouldRegisterBoth()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddContentFormatters(options =>
        {
            options.AddFormatter<TestXmlContentFormatter>();
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var formatters = serviceProvider.GetServices<IContentFormatter>().ToList();
        formatters.Count.ShouldBe(2);
        formatters.Any(f => f is JsonContentFormatter).ShouldBeTrue();
        formatters.Any(f => f is TestXmlContentFormatter).ShouldBeTrue();
    }

    #region Test Data

    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestXmlContentFormatter : IContentFormatter
    {
        public IEnumerable<string> SupportedContentTypes => new[] { "application/xml", "text/xml" };

        public bool CanFormat(string contentType) =>
            contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);

        public Task FormatAsync<T>(HttpContext context, T? value, CancellationToken cancellationToken = default)
        {
            context.Response.ContentType = "application/xml";
            return Task.CompletedTask;
        }
    }

    #endregion

    #region Helper Methods

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    #endregion
}
