namespace DotNetBrightener.Core.BackgroundTasks;

public interface IScheduleConfig
{

    /// <summary>
    ///     Scheduled task runs every minute.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryMinute();

    /// <summary>
    ///     Scheduled task runs every five minutes.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryFiveMinutes();

    /// <summary>
    ///     Scheduled task runs every ten minutes.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryTenMinutes();

    /// <summary>
    ///     Scheduled task runs every fifteen minutes.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryFifteenMinutes();

    /// <summary>
    ///     Scheduled task runs every thirty minutes.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryThirtyMinutes();

    /// <summary>
    ///     Scheduled task runs once an hour.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Hourly();

    /// <summary>
    ///     Scheduled task runs once an hour, but only at the time specified.
    /// </summary>
    /// <example>
    ///     HourlyAt(14); // Will run once an hour at xx:14.
    /// </example>
    /// <param name="minute">Minute each hour that task will run.</param>
    /// <returns></returns>
    IScheduleConfig HourlyAt(int minute);

    /// <summary>
    ///     Scheduled task runs once a day.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Daily();

    /// <summary>
    ///     Scheduled task runs once a day at the hour specified.
    /// </summary>
    /// <example>
    ///     DailyAtHour(13); // Run task daily at 1 pm utc.
    /// </example>
    /// <param name="hour">Task only runs at this hour.</param>
    /// <returns></returns>
    IScheduleConfig DailyAtHour(int hour);

    /// <summary>
    ///     Scheduled task runs once a day at the time specified.
    /// </summary>
    /// <example>
    ///     DailyAt(13, 01); // Run task daily at 1:01 pm utc.
    /// </example>
    /// <param name="hour">Task only runs at this hour.</param>
    /// <param name="minute">Task only runs at time with this minute.</param>
    /// <returns></returns>
    IScheduleConfig DailyAt(int hour, int minute);

    /// <summary>
    /// Scheduled task runs once a week.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Weekly();

    /// <summary>
    /// Scheduled task runs once a month.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Monthly();

    /// <summary>
    /// Schedule an event from a basic cron expression.
    /// Supported values for expression parts are:
    /// - "*"
    /// - "5"
    /// - "5,6,7"
    /// - "5-10"
    /// - "*/10"
    /// 
    /// For example "* * * * 0" would schedule an event to run every minute on Sundays.
    /// </summary>
    /// <param name="cronExpression"></param>
    /// <returns></returns>
    IScheduleConfig Cron(string cronExpression);

    /// <summary>
    /// Scheduled task runs once a second.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EverySecond();

    /// <summary>
    /// Scheduled task runs once every five seconds.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryFiveSeconds();

    /// <summary>
    ///     Scheduled task runs once every ten seconds.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryTenSeconds();

    /// <summary>
    ///     Scheduled task runs once every fifteen seconds.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryFifteenSeconds();

    /// <summary>
    ///     Scheduled task runs once every thirty seconds.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EveryThirtySeconds();

    /// <summary>
    ///     Scheduled task runs once every N seconds.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig EverySeconds(int seconds);

    /// <summary>
    /// Restrict task to run on Mondays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Monday();

    /// <summary>
    /// Restrict task to run on Tuesdays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Tuesday();

    /// <summary>
    /// Restrict task to run on Wednesdays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Wednesday();

    /// <summary>
    /// Restrict task to run on Thursdays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Thursday();

    /// <summary>
    /// Restrict task to run on Fridays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Friday();

    /// <summary>
    /// Restrict task to run on Saturdays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Saturday();

    /// <summary>
    /// Restrict task to run on Sundays.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Sunday();

    /// <summary>
    ///     Restrict task to run on weekdays (Monday - Friday).
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Weekday();

    /// <summary>
    ///     Restricts task to run on weekends (Saturday and Sunday).
    /// </summary>
    /// <returns></returns>
    IScheduleConfig Weekend();

    /// <summary>
    ///     If this event has not completed from the last time it was invoked, and is due again,
    ///     it will be prevented from running.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig PreventOverlapping(string uniqueIdentifier = null);

    /// <summary>
    /// Restrict scheduled task to run only when result of <paramref name="predicate"/> is true.
    /// </summary>
    /// <returns></returns>
    IScheduleConfig When(Func<Task<bool>> predicate);

    /// <summary>
    ///     Specifies the time zone for the schedule to run in.
    /// </summary>
    /// <param name="timeZoneInfo"></param>
    /// <returns></returns>
    IScheduleConfig AtTimeZone(TimeZoneInfo timeZoneInfo);

    /// <summary>
    ///     Specifies that the task will run once after scheduled
    /// </summary>
    IScheduleConfig Once();
}