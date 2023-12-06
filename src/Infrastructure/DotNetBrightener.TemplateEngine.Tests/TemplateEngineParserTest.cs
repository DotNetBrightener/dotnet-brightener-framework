using System;
using DotNetBrightener.TemplateEngine.Helpers;
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

        serviceCollection.AddScoped<ITemplateParserService, TemplateParserService>();

        serviceCollection.AddScoped<ITemplateHelperRegistration, TemplateHelperRegistration>();
        serviceCollection.AddScoped<ITemplateParserService, TemplateParserService>();

        serviceCollection.AddTemplateHelperProvider<DateTimeTemplateHelper>();
        serviceCollection.AddTemplateHelperProvider<FormatCurrencyTemplateHelper>();
        serviceCollection.AddTemplateHelperProvider<SumTemplateHelper>();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        
        var templateHelperRegistration = _serviceProvider.GetService<ITemplateHelperRegistration>();

        templateHelperRegistration?.RegisterHelpers();
    }

    [Test]
    public void TestParseTemplate()
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var result = templateParserService.ParseTemplate("{{#each this}}<p>{{this}}</p>{{/each}}",
                                                         new[]
                                                         {
                                                             "111 Adam's MHS Test St., NorthPole, AK 66666"
                                                         });
        Assert.AreEqual("<p>111 Adam's MHS Test St., NorthPole, AK 66666</p>", result);
    }

    [Test]
    public void TestParseTemplate_ComplexObject()
    {
        var templateParserService = _serviceProvider.GetService<ITemplateParserService>();

        var result = templateParserService.ParseTemplate("Welcome to your home. {{Address}}",
                                                         new
                                                         {
                                                             Address = "111 Adam’s MHS Test St., NorthPole, AK 66666"
                                                         });
        Assert.AreEqual("Welcome to your home. 111 Adam's MHS Test St., NorthPole, AK 66666", result);
    }
}