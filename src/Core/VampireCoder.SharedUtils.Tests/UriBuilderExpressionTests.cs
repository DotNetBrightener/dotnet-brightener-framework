using System.Web;
using Xunit.Abstractions;

namespace VampireCoder.SharedUtils.Tests;

public class UriBuilderExpressionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UriBuilderExpressionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("http://localhost:8080/whatever/path", "localhost:8080")]
    [InlineData("https://www.google.com", "www.google.com")]
    public void TestGetDomain(string inputUrl, string expectedDomainValue)
    {
        var expression = new Uri(inputUrl);

        var parsedData = expression.GetDomain();

        Assert.Equal(expectedDomainValue, parsedData);
    }

    [Theory]
    [InlineData("http://localhost:8080/whatever/path", "http://localhost:8080")]
    [InlineData("https://www.google.com/search?s=whatever", "https://www.google.com")]
    public void TestGetBaseUrl(string inputUrl, string expectedDomainValue)
    {
        var expression = new Uri(inputUrl);

        var parsedData = expression.GetBaseUrl();

        Assert.Equal(expectedDomainValue, parsedData);
    }
}