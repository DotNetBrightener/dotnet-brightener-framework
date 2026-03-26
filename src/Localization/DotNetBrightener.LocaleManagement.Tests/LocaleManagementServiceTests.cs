using LocaleManagement.Services;
using Xunit;

namespace LocaleManagement.Tests;

public class LocaleManagementServiceTests
{
    public LocaleManagementServiceTests()
    {
    }

    [Fact]
    public async Task GetSystemSupportedLocales_ShouldBeAbleToGetSystemLocales()
    {
        var supportedLocales = LocaleManagementService.InternalGetSystemSupportedLocales();

        Assert.NotNull(supportedLocales);

        //Console.WriteLine(JsonConvert.SerializeObject(supportedLocales, Formatting.Indented));
    }
}
