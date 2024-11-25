using LocaleManagement.Services;
using NUnit.Framework;

namespace LocaleManagement.Tests;

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

        //Console.WriteLine(JsonConvert.SerializeObject(supportedLocales, Formatting.Indented));
    }

    [TearDown]
    public void TearDown()
    {
    }
}