using DotNetBrightener.Core.BackgroundTasks.Cron;
using Xunit.Abstractions;

namespace DotNetBrightener.Core.BackgroundTasks.Tests;

public class CronExpressionParser
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CronExpressionParser(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    // Always
    [InlineData("* * * * *", "At 00 seconds past the minute")]
    [InlineData("* * * 1-10/3 *", "At 00 seconds past the minute, every 3 months, January through October")]
    [InlineData("* 1 * * *", "At 00 seconds past the minute, between 01:00 AM and 01:59 AM")]
    [InlineData("* * 1 * *", "At 00 seconds past the minute, on day 1 of the month")]
    [InlineData("* * * * */2", "At 00 seconds past the minute, every 2 days of the week")]
    [InlineData("1-10/3 5-10/5 */2 3-4/1 *", "At 00 seconds past the minute, every 3 minutes, minutes 1 through 10 past the hour, every 5 hours, between 05:00 AM and 10:59 AM, every 2 days, every 1 months, March through April")]
    public void ParseCronExpression(string cronExpression, string expectedResult)
    {
        var expression = new CronExpression(cronExpression);

        var parsedData = expression.ToString();

        _testOutputHelper.WriteLine(parsedData);

        Assert.Equal(expectedResult, parsedData);
    }
}
