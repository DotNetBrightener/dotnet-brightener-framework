using DotNetBrightener.Caching;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.LocaleManagement.Data;
using DotNetBrightener.LocaleManagement.Models;
using DotNetBrightener.LocaleManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetBrightener.LocaleManagement.Tests;

public class LocaleManagementServiceTests
{

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task GetSystemSupportedLocales_ShouldBeAbleToGetSystemLocales()
    {
        var supportedLocales = LocaleManagementService.InternalGetSystemSupportedLocales();

        Assert.That(supportedLocales, Is.Not.Null);

        Console.WriteLine(JsonConvert.SerializeObject(supportedLocales, Formatting.Indented));
    }

    [TearDown]
    public void TearDown()
    {
    }
}