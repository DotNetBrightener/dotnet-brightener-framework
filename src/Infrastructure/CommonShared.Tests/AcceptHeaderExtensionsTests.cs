using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Shouldly;
using WebApp.CommonShared.Endpoints.Response;
using Xunit;

namespace WebApp.CommonShared.Tests;

/// <summary>
///     Unit tests for Accept header parsing and matching
/// </summary>
public class AcceptHeaderExtensionsTests
{
    [Fact]
    public void GetAcceptedContentTypes_WithNoAcceptHeader_ShouldReturnDefaultJson()
    {
        // Arrange
        var request = CreateRequest("");

        // Act
        var types = request.GetAcceptedContentTypes();

        // Assert
        types.Count.ShouldBe(1);
        types[0].MediaType.ShouldBe("application/json");
        types[0].Quality.ShouldBe(1.0);
    }

    [Fact]
    public void GetAcceptedContentTypes_WithSingleAcceptHeader_ShouldParseCorrectly()
    {
        // Arrange
        var request = CreateRequest("application/json");

        // Act
        var types = request.GetAcceptedContentTypes();

        // Assert
        types.Count.ShouldBe(1);
        types[0].MediaType.ShouldBe("application/json");
        types[0].Quality.ShouldBe(1.0);
    }

    [Fact]
    public void GetAcceptedContentTypes_WithMultipleAcceptHeaders_ShouldOrderByQuality()
    {
        // Arrange
        var request = CreateRequest("text/html;q=0.8, application/json;q=0.9, application/xml;q=0.5");

        // Act
        var types = request.GetAcceptedContentTypes();

        // Assert
        types.Count.ShouldBe(3);
        types[0].MediaType.ShouldBe("application/json");
        types[0].Quality.ShouldBe(0.9);
        types[1].MediaType.ShouldBe("text/html");
        types[1].Quality.ShouldBe(0.8);
        types[2].MediaType.ShouldBe("application/xml");
        types[2].Quality.ShouldBe(0.5);
    }

    [Fact]
    public void GetAcceptedContentTypes_WithWildcard_ShouldParseCorrectly()
    {
        // Arrange
        var request = CreateRequest("*/*");

        // Act
        var types = request.GetAcceptedContentTypes();

        // Assert
        types.Count.ShouldBe(1);
        types[0].MediaType.ShouldBe("*/*");
    }

    [Theory]
    [InlineData("application/json", "application/json", true)]
    [InlineData("application/json", "text/html", false)]
    [InlineData("*/*", "application/json", true)]
    [InlineData("*/*", "text/xml", true)]
    [InlineData("application/*", "application/json", true)]
    [InlineData("application/*", "application/xml", true)]
    [InlineData("application/*", "text/html", false)]
    [InlineData("text/*", "application/json", false)]
    [InlineData("text/*", "text/html", true)]
    public void AcceptMediaType_Matches_ShouldReturnExpectedResult(
        string acceptType,
        string contentType,
        bool expected)
    {
        // Arrange
        var mediaType = new AcceptMediaType(acceptType, 1.0);

        // Act
        var result = mediaType.Matches(contentType);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void AcceptsContentType_WhenTypeAccepted_ShouldReturnTrue()
    {
        // Arrange
        var request = CreateRequest("application/json, text/html");

        // Act
        var result = request.AcceptsContentType("application/json");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AcceptsContentType_WhenTypeNotAccepted_ShouldReturnFalse()
    {
        // Arrange
        var request = CreateRequest("application/json, text/html");

        // Act
        var result = request.AcceptsContentType("application/xml");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AcceptsContentType_WithWildcard_ShouldReturnTrue()
    {
        // Arrange
        var request = CreateRequest("*/*");

        // Act
        var result = request.AcceptsContentType("application/json");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GetBestContentType_WithMatchingType_ShouldReturnBestMatch()
    {
        // Arrange
        var request = CreateRequest("application/json, text/html;q=0.8");
        var supportedTypes = new[] { "text/html", "application/json" };

        // Act
        var best = request.GetBestContentType(supportedTypes);

        // Assert
        best.ShouldBe("application/json"); // Higher quality (1.0 vs 0.8)
    }

    [Fact]
    public void GetBestContentType_WithNoMatch_ShouldReturnFirstSupportedType()
    {
        // Arrange
        var request = CreateRequest("application/xml");
        var supportedTypes = new[] { "application/json", "text/html" };

        // Act
        var best = request.GetBestContentType(supportedTypes);

        // Assert
        best.ShouldBe("application/json"); // Fallback to first supported
    }

    [Fact]
    public void GetBestContentType_WithWildcard_ShouldReturnFirstSupportedType()
    {
        // Arrange
        var request = CreateRequest("*/*");
        var supportedTypes = new[] { "application/json", "text/html" };

        // Act
        var best = request.GetBestContentType(supportedTypes);

        // Assert
        best.ShouldBe("application/json");
    }

    [Fact]
    public void AcceptMediaType_ToString_ShouldIncludeQuality()
    {
        // Arrange & Act
        var mediaType = new AcceptMediaType("application/json", 0.8);

        // Act
        var result = mediaType.ToString();

        // Assert
        result.ShouldContain("application/json");
        result.ShouldContain("0.8");
    }

    #region Helper Methods

    private static HttpRequest CreateRequest(string acceptHeader)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = new StringValues(acceptHeader);
        return context.Request;
    }

    #endregion
}
