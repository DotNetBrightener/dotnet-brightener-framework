namespace DotNetBrightener.Core.BackgroundTasks.Cron;

/// <summary>
///     Define an expression to describe a cron job
/// </summary>
/// <remarks>
///     A cron syntax has the following format:                          <br /> 
///      -------------------------- Seconds (0 - 59) (Can be omitted)      <br />
///     |  ------------------------ Minutes (0 - 59)                       <br />
///     | |  ---------------------- Hours (0 - 23)                         <br />
///     | | |  -------------------- Days (1 - 31)                          <br />
///     | | | |  ------------------ Months (1 - 12)                        <br />
///     | | | | |  ---------------- Weekdays (0 - 6) (Sunday=0)            <br />
///     | | | | | |                                                        <br />
///     * * * * * *                                                        <br />
///<br /><br />
/// For example: <br />
///     - "0 0 1 * *" means "At 00:00 on day-of-month 1."<br />
///     - "0 0 * * *" means "At 00:00."
/// </remarks>
public class CronExpression
{
    private string _seconds;
    private string _minutes;
    private string _hours;
    private string _days;
    private string _months;
    private string _weekdays;

    public CronExpression(string expression)
    {
        var values = expression.Split(' ');

        if (values.Length < 5 ||
            values.Length > 6 || 
            values.Any(string.IsNullOrEmpty))
        {
            throw new InvalidCronExpressionException($"Cron expression '{expression}' is not valid.");
        }

        if (values.Length == 5)
        {
            _seconds  = "00";
            _minutes  = values[0];
            _hours    = values[1];
            _days     = values[2];
            _months   = values[3];
            _weekdays = values[4];
        }

        else if (values.Length == 6)
        {
            _seconds  = values[0];
            _minutes  = values[1];
            _hours    = values[2];
            _days     = values[3];
            _months   = values[4];
            _weekdays = values[5];
        }

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
            _weekdays += $",{intDay}";
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
        if (string.IsNullOrEmpty(_seconds))
            return IsMinuteDue(time)
                && IsHoursDue(time)
                && IsDayDue(time)
                && IsMonthDue(time)
                && IsWeekDayDue(time);

        return IsSecondDue(time)
            && IsMinuteDue(time)
            && IsHoursDue(time)
            && IsDayDue(time)
            && IsMonthDue(time)
            && IsWeekDayDue(time);
    }

    public bool IsWeekDayDue(DateTime time)
    {
        return new CronExpressionPart(_weekdays, 7).IsDue((int)time.DayOfWeek);
    }

    private bool IsSecondDue(DateTime time)
    {
        return new CronExpressionPart(_seconds, 60).IsDue(time.Second);
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
        var time = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(_seconds)) IsSecondDue(time);

        IsMinuteDue(time);
        IsHoursDue(time);
        IsDayDue(time);
        IsMonthDue(time);
        IsWeekDayDue(time);
    }

    public override string ToString()
    {
        List<string> cronParts = [_seconds, _minutes, _hours, _days, _months, _weekdays];

        var cronExpression = string.Join(" ", cronParts.Where(p => !string.IsNullOrEmpty(p)))
                                   .Trim();

        return CronExpressionDescriptor.ExpressionDescriptor
                                       .GetDescription(cronExpression);
    }
}