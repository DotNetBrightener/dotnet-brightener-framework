using DotNetBrightener.Core.Logging;
using NUnit.Framework;

public class FormattedLogValueTest
{
    [Test]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateWithoutRetrievingData()
    {
        var formatString = "Hello {name}";

        var logValue = new FormattedLogValues(formatString, "World");

        var dictionary = logValue.Values;

        Assert.That(logValue.ToString(), Is.EqualTo("Hello World"));
    }
}