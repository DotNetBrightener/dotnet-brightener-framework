using DotNetBrightener.DataAccess.EF.Internal;
using System.Linq.Expressions;
using FluentAssertions;
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
    public DateTimeOffset? DateValue { get; set; }

}

public class QueryableExtensionsTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ExtractFiltersTest()
    {
        var userId = 10;
        var preferenceKey = "Theme";

        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => (p.UserId == userId &&
                                                                           p.PreferenceKey == preferenceKey) ||
                                                                          p.DateValue < DateTimeOffset.Now.AddDays(-10);

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));

    }

    [Fact]
    public void ExtractFilters_ContainsOperation_NonNullableType_ShouldReturnCorrectly()
    {
        var userIds = new List<long>([1, 2, 5, 6, 10, 20]);

        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => userIds.Contains(p.UserId);

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }

    [Fact]
    public void ExtractFilters_ContainsOperation_NullableType_ShouldReturnCorrectly()
    {
        var userIds = new List<long?>([1, 2, 5, 6, 10, 20]);

        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => userIds.Contains(p.UserId);

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }

    [Fact]
    public void ExtractFilters_ComplexOperations_ShouldReturnCorrectly()
    {
        List<long> userIds = [1, 2, 5, 6, 10, 20];

        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression;
        conditionExpression = p => userIds.Contains(p.UserId) && 
                                   string.IsNullOrEmpty(p.PreferenceValue);

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));

        conditionExpression = p => userIds.Contains(p.UserId) || 
                                   string.IsNullOrEmpty(p.PreferenceValue);

        // Act
        result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }

    [Fact]
    public void ExtractFilters_ComplexOperations_NegativeCheck_ShouldReturnCorrectly()
    {
        List<long> userIds = [1, 2, 5, 6, 10, 20];

        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => userIds.Contains(p.UserId) && 
                                                                          !string.IsNullOrEmpty(p.PreferenceValue);

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }

    [Fact]
    public void ExtractFilters_ContainsStringOperation_ShouldReturnCorrectly()
    {
        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => p.PreferenceValue.Contains("someData");

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }

    [Fact]
    public void ExtractFilters_StartsWithOperation_ShouldReturnCorrectly()
    {
        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => p.PreferenceValue.StartsWith("someData");

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }

    [Fact]
    public void ExtractFilters_EndsWithOperation_ShouldReturnCorrectly()
    {
        // Arrange
        Expression<Func<UserPreference, bool>> conditionExpression = p => p.PreferenceValue.EndsWith("someData");

        // Act
        var result = conditionExpression.ExtractFilters();
        testOutputHelper.WriteLine(result.Serialize(true));
    }
}