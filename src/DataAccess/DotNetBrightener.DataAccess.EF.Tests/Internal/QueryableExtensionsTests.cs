using DotNetBrightener.DataAccess.EF.Internal;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.Tests.Internal;

internal class UserPreference
{
    public long UserId { get; set; }

    public string PreferenceKey { get; set; }

    public string PreferenceValue { get; set; }

    public string PreferenceType { get; set; }

}

public class QueryableExtensionsTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ExtractFiltersTest()
    {
        var userId = 10;
        var preferenceKey = "Theme";

        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => p.UserId == userId &&
                                                                          p.PreferenceKey == preferenceKey;

        // Act
        var result = conditionExpression.ExtractFilters();

        testOutputHelper.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }
}