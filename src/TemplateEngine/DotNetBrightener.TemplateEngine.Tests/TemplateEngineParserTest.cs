using DotNetBrightener.TemplateEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DotNetBrightener.TemplateEngine.Tests;

[TestFixture]
public class TemplateEngineParserTest
{
    private IServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTemplateEngine();

        _serviceProvider = serviceCollection.BuildServiceProvider();


        var templateHelperRegistration = _serviceProvider.GetService<ITemplateHelperRegistration>();

        templateHelperRegistration?.RegisterHelpers();
    }

    [Test]
    [TestCase("’", "'")]
    [TestCase("&", "&")]
    [TestCase("\"", "\"")]
    [TestCase("<", "<")]
    [TestCase(">", ">")]
    [TestCase("€", "€")]
    [TestCase("£", "£")]
    [TestCase("®", "®")]
    [TestCase("©", "©")]
    public void TestParseTemplate_HtmlDisabled_ShouldRetainInputAsItWas(string input, string expectedValue)
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var parsedValue = templateParserService.ParseTemplate("{{this}}", input, false);
        Assert.That(parsedValue, Is.EqualTo(expectedValue));
    }
    
    [Test]
    [TestCase("Welcome to your home. {{Address}}",
              "111 Adam’s MHS Test St., NorthPole, AK 66666",
              "Welcome to your home. 111 Adam's MHS Test St., NorthPole, AK 66666")]
    public void TestParseTemplate_ComplexObject(string template,
                                                string inputAddress,
                                                string expectedResult)
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var result = templateParserService.ParseTemplate(template,
                                                         new
                                                         {
                                                             Address = inputAddress
                                                         },
                                                         isHtml: false);
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    [TestCase("Welcome to your home. {{Address}}",
              "111 Adam’s MHS Test St., NorthPole, AK 66666",
              "Welcome to your home. 111 Adam's MHS Test St., NorthPole, AK 66666")]
    public void TestParseTemplate_ComplexObject_HtmlEnabled(string template,
                                                string inputAddress,
                                                string expectedResult)
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var result = templateParserService.ParseTemplate(template,
                                                         new
                                                         {
                                                             Address = inputAddress
                                                         },
                                                         isHtml: true);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    [TestCase("{{formatDate Date}}",
              "2024-04-01 12:00:00",
              "2024-04-01T12:00:00.0000000")]
    [TestCase("{{formatDate Date 'MMM dd, yyyy'}}",
              "2024-04-01 12:00:00",
              "Apr 01, 2024")]
    [TestCase("{{formatDate Date MMM dd, yyyy}}",
              "2024-04-01 12:00:00",
              "Apr 01, 2024")]
    [TestCase("{{formatDate Date MMM dd, yyyy HH:mm}}",
              "2024-04-01 13:20:00",
              "Apr 01, 2024 13:20")]
    [TestCase("{{formatDate Date MMM dd, yyyy hh:mm tt}}",
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
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}