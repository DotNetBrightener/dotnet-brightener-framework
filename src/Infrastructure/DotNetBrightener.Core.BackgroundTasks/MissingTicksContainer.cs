namespace DotNetBrightener.Core.BackgroundTasks;

public static class DateHelpers
{
    public static DateTime Clone(this DateTime me)
    {
        return new DateTime(me.Year, me.Month, me.Day, me.Hour, me.Minute, me.Second, DateTimeKind.Utc);
    }
}

/// <summary>
///     Represents the bucket to store the missing ticks between two given times;
/// </summary>
public class MissingTicksContainer(DateTime previousTick)
{
    /// <summary>
    ///     Retrieve the missing tickets between the <see cref="previousTick"/> and the given time
    /// </summary>
    /// <param name="nextTick">
    ///     The new tick to check for missing ticks
    /// </param>
    /// <returns></returns>
    public IEnumerable<DateTime> RetrieveMissingTicksUntil(DateTime nextTick)
    {
        List<DateTime> missingTicks = null;

        var nextTickToTest = previousTick.Clone()
                                         .AddSeconds(1);

        while (nextTickToTest < nextTick.Clone())
        {
            if (missingTicks is null)
            {
                missingTicks = new List<DateTime>();
            }

            missingTicks.Add(nextTickToTest);

            nextTickToTest = nextTickToTest.Clone()
                                           .AddSeconds(1);
        }

        return missingTicks ?? Enumerable.Empty<DateTime>();
    }

    /// <summary>
    ///     Switch the previous tick to the new one
    /// </summary>
    /// <param name="nextTick">The new tick to replace the old one</param>
    public void MoveTo(DateTime nextTick)
    {
        previousTick = nextTick;
    }
}