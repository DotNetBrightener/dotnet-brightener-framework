namespace System;

public interface IDateTimeProvider
{
    DateTime Now { get; }

    DateTime UtcNow { get; }

    DateTime UnixEpoch { get; }
}

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime UnixEpoch => DateTime.UnixEpoch;
}
