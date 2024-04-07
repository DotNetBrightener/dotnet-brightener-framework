namespace DotNetBrightener.Core.BackgroundTasks.Cron;

/// <summary>
///     Define an expression to describe a cron job
/// </summary>
/// <remarks>
///     A cron syntax has the following format:                          <br />
///      ------------------------ Minutes (0 - 59)                       <br />
///     |  ---------------------- Hours (0 - 23)                         <br />
///     | |  -------------------- Days (1 - 31)                          <br />
///     | | |  ------------------ Months (1 - 12)                        <br />
///     | | | |  ---------------- Weekdays (0 - 6) (Sunday=0)            <br />
///     | | | | |                                                        <br />
///     * * * * *                                                        <br />
///<br /><br />
/// For example: <br />
///     - "0 0 1 * *" means "At 00:00 on day-of-month 1."<br />
///     - "0 0 * * *" means "At 00:00."
/// </remarks>
public class CronExpression
{
    private string _minutes;
    private string _hours;
    private string _days;
    private string _months;
    private string _weekdays;

    private CronExpression(string expression)
    {
        var values = expression.Split(' ');
        if (values.Length != 5)
        {
            throw new InvalidCronExpressionException($"Cron expression '{expression}' is not valid.");
        }

        _minutes = values[0];
        _hours = values[1];
        _days = values[2];
        _months = values[3];
        _weekdays = values[4];

        GuardExpressionIsValid();
    }

    /// <summary>
    ///     Parse and validate the cron expression
    /// </summary>
    /// <param name="expression">The cron expression in string</param>
    /// <returns>
    ///     The <see cref="CronExpression"/> object
    /// </returns>
    public static CronExpression Parse(string expression)
    {
        return new CronExpression(expression);
    }

    public CronExpression AppendWeekDay(DayOfWeek day)
    {
        int intDay = (int)day;

        if (_weekdays == "*")
        {
            _weekdays = intDay.ToString();
        }
        else
        {
            _weekdays += "," + intDay.ToString();
        }

        return this;
    }

    /// <summary>
    ///     Checks if the cron expression is due at the given time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool IsDue(DateTime time)
    {
        return IsMinuteDue(time)
            && IsHoursDue(time)
            && IsDayDue(time)
            && IsMonthDue(time)
            && IsWeekDayDue(time);
    }

    public bool IsWeekDayDue(DateTime time)
    {
        return new CronExpressionPart(_weekdays, 7).IsDue((int)time.DayOfWeek);
    }

    private bool IsMinuteDue(DateTime time)
    {
        return new CronExpressionPart(_minutes, 60).IsDue(time.Minute);
    }

    private bool IsHoursDue(DateTime time)
    {
        return new CronExpressionPart(_hours, 24).IsDue(time.Hour);
    }

    private bool IsDayDue(DateTime time)
    {
        return new CronExpressionPart(_days, 31).IsDue(time.Day);
    }

    private bool IsMonthDue(DateTime time)
    {
        return new CronExpressionPart(_months, 12).IsDue(time.Month);
    }

    private void GuardExpressionIsValid()
    {
        // We don't want to check that the expression is due, but just run validation and ignore any results.
        var time = DateTime.UtcNow;
        IsMinuteDue(time);
        IsHoursDue(time);
        IsDayDue(time);
        IsMonthDue(time);
        IsWeekDayDue(time);
    }
}