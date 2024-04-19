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
    [InlineData("* * * * *", "Every minute")]
    [InlineData("* * * 1-10/3 *", "Every minute, every 3 months, January through October")]
    [InlineData("* 1 * * *", "Every minute, between 01:00 AM and 01:59 AM")]
    [InlineData("* * 1 * *", "Every minute, on day 1 of the month")]
    [InlineData("* * * * */2", "Every minute, every 2 days of the week")]
    [InlineData("1-10/3 5-10/5 */2 3-4/1 *", "Every minute, every 2 days of the week")]
    public void ParseCronExpression(string cronExpression, string expectedResult)
    {
        var expression = new CronExpression(cronExpression);

        var parsedData = expression.ToString();

        _testOutputHelper.WriteLine(parsedData);

        Assert.Equal(expectedResult, parsedData);
    }
}