using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.CryptoEngine.Tests;

public class CryptoUtilitiesTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void GenerateTimeBasedTokenTest_ShouldBeValid()
    {
        var input = "Hello world! This is message";

        var token = CryptoUtilities.GenerateTimeBasedToken(ref input);

        var validated = CryptoUtilities.ValidateTimeBasedToken(token, out var output);

        validated.ShouldBe(true);

        output.ShouldBe(input);
    }

    [Fact]
    public void GenerateTimeBasedTokenTest_ShouldBeNotValid()
    {
        var input = "Hello world! This is message";

        var token = CryptoUtilities.GenerateTimeBasedToken(ref input, TimeSpan.FromMinutes(-5));

        var validated = CryptoUtilities.ValidateTimeBasedToken(token, out var output);

        validated.ShouldBe(false);
    }
}