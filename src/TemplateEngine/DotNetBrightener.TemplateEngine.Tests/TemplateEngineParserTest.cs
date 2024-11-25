using DotNetBrightener.TemplateEngine.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DotNetBrightener.TemplateEngine.Tests;

public class TemplateEngineParserTest
{
    private readonly IServiceProvider  _serviceProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public TemplateEngineParserTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTemplateEngine();

        _serviceProvider = serviceCollection.BuildServiceProvider();


        var templateHelperRegistration = _serviceProvider.GetService<ITemplateHelperRegistration>();

        templateHelperRegistration?.RegisterHelpers();
    }

    [Theory]
    //[TestCase("’", "'")]
    [InlineData("&", "&")]
    [InlineData("\"", "\"")]
    [InlineData("<", "<")]
    [InlineData(">", ">")]
    [InlineData("€", "€")]
    [InlineData("£", "£")]
    [InlineData("®", "®")]
    [InlineData("©", "©")]
    public void TestParseTemplate_HtmlDisabled_ShouldRetainInputAsItWas(string input, string expectedValue)
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var parsedValue = templateParserService.ParseTemplate("{{this}}", input, false);
        parsedValue.Should().Be(expectedValue);
    }

    //[Test]
    //[TestCase("Welcome to your home. {{Address}}",
    //          "111 Adam’s MHS Test St., NorthPole, AK 66666",
    //          "Welcome to your home. 111 Adam's MHS Test St., NorthPole, AK 66666")]
    //public void TestParseTemplate_ComplexObject(string template,
    //                                            string inputAddress,
    //                                            string expectedResult)
    //{
    //    var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

    //    var result = templateParserService.ParseTemplate(template,
    //                                                     new
    //                                                     {
    //                                                         Address = inputAddress
    //                                                     },
    //                                                     isHtml: false);
    //    Assert.That(result, Is.EqualTo(expectedResult));
    //}

    //[Test]
    //[TestCase("Welcome to your home. {{Address}}",
    //          "111 Adam’s MHS Test St., NorthPole, AK 66666",
    //          "Welcome to your home. 111 Adam's MHS Test St., NorthPole, AK 66666")]
    //public void TestParseTemplate_ComplexObject_HtmlEnabled(string template,
    //                                            string inputAddress,
    //                                            string expectedResult)
    //{
    //    var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

    //    var result = templateParserService.ParseTemplate(template,
    //                                                     new
    //                                                     {
    //                                                         Address = inputAddress
    //                                                     },
    //                                                     isHtml: true);
    //    Assert.That(result, Is.EqualTo(expectedResult));
    //}

    [Theory]
    [InlineData("{{formatDate Date}}",
                "2024-04-01 12:00:00",
                "2024-04-01T12:00:00.0000000")]
    [InlineData("{{formatDate Date 'MMM dd, yyyy'}}",
                "2024-04-01 12:00:00",
                "Apr 01, 2024")]
    [InlineData("{{formatDate Date MMM dd, yyyy}}",
                "2024-04-01 12:00:00",
                "Apr 01, 2024")]
    [InlineData("{{formatDate Date MMM dd, yyyy HH:mm}}",
                "2024-04-01 13:20:00",
                "Apr 01, 2024 13:20")]
    [InlineData("{{formatDate Date MMM dd, yyyy hh:mm tt}}",
                "2024-04-01 13:20:00",
                "Apr 01, 2024 01:20 PM")]
    public void TestParseTemplate_FormatDateHelper(string template,
                                                   string dateInput,
                                                   string expectedResult)
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var result = templateParserService.ParseTemplate(template,
                                                         new
                                                         {
                                                             Date = DateTime.Parse(dateInput)
                                                         },
                                                         isHtml: false);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("{{formatCurrency Price}}",
                10.00,
                "$10.00")]
    [InlineData("{{formatCurrency Price 'C'}}",
                10.30,
                "$10.30")]
    [InlineData("{{formatCurrency Price '\u00a3'}}",
                10.00,
                "\u00a310")]
    [InlineData("{{formatCurrency Price '\u00a30.00'}}",
                10.00,
                "\u00a310.00")]
    [InlineData("{{formatCurrency Price '\u00a30.00'}}",
                10.30,
                "\u00a310.30")]
    [InlineData("{{formatCurrency Price '' 'en-US'}}",
                10.00,
                "$10.00")]
    [InlineData("{{formatCurrency Price '' 'en-US'}}",
                10.30,
                "$10.30")]
    [InlineData("{{formatCurrency Price '' 'en-GB'}}",
                10.30,
                "\u00a310.30")]
    [InlineData("{{formatCurrency Price '' 'en-VN'}}",
                10300,
                "\u20ab10,300")]
    [InlineData("{{formatCurrency Price '' 'vi-VN'}}",
                10300,
                "10.300 \u20ab")]
    public void TestParseTemplate_FormatCurrencyHelper(string  template,
                                                       decimal priceInput,
                                                       string  expectedResult)
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var result = templateParserService.ParseTemplate(template,
                                                         new
                                                         {
                                                             Price = priceInput
                                                         },
                                                         isHtml: false);
        _testOutputHelper.WriteLine("Output: " + result);
        result.Should().Be(expectedResult);
    }
}