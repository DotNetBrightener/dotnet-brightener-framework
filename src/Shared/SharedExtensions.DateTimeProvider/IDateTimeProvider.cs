namespace System;

public interface IDateTimeProvider
{
    DateTime       Now              { get; }
    DateTimeOffset NowWithOffset    { get; }
    DateTime       UtcNow           { get; }
    DateTimeOffset UtcNowWithOffset { get; }
    DateTime       UnixEpoch        { get; }
}

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime       Now              => DateTime.Now;
    public DateTimeOffset NowWithOffset    => DateTimeOffset.Now;
    public DateTime       UtcNow           => DateTime.UtcNow;
    public DateTimeOffset UtcNowWithOffset => DateTimeOffset.UtcNow;
    public DateTime       UnixEpoch        => DateTime.UnixEpoch;
}