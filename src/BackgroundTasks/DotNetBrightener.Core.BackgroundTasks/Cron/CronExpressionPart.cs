namespace DotNetBrightener.Core.BackgroundTasks.Cron;

/// <summary>
///     Represents a part of the cron expression to check when it is due
/// </summary>
public class CronExpressionPart(string expression, int replaceZeroWith)
{
    /// <summary>
    ///     The cron expression used to determine when event is due
    /// </summary>
    private readonly string _expression = expression.Trim();

    /// <summary>
    ///     Checks if the cron expression is due at the given time value
    /// </summary>
    /// <param name="time">
    ///     The time value to check
    /// </param>
    /// <returns>
    ///     <c>true</c> if the cron expression is due at the given time value; otherwise, <c>false</c>
    /// </returns>
    /// <exception cref="InvalidCronExpressionException"></exception>
    public bool IsDue(int time)
    {
        if (_expression == "*")
        {
            return true;
        }

        var isDivisibleUnit = _expression.IndexOf("*/") > -1;

        if (isDivisibleUnit)
        {
            if (!int.TryParse(_expression.Remove(0, 2), out var divisor))
            {
                throw new InvalidCronExpressionException($"Cron entry '{_expression}' is invalid.");
            }

            if (divisor == 0)
            {
                throw new InvalidCronExpressionException($"Cron entry ${_expression} is attempting division by zero.");
            }

            if (time == 0)
            {
                time = replaceZeroWith;
            }

            return time % divisor == 0;
        }

        return new CronExpressionComplexPart(_expression).CheckIfTimeIsDue(time);
    }
}