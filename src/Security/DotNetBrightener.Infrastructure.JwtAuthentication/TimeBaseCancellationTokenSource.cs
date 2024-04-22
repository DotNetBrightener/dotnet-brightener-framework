namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public class TimeBaseCancellationTokenSource : CancellationTokenSource
{
    private readonly Timer    _timer;

    public           TimeSpan Duration { get; private set; }

    public TimeBaseCancellationTokenSource(TimeSpan duration)
    {
        Duration = duration;

        _timer = new Timer(state =>
        {
            ((CancellationTokenSource)state!).Cancel();
        }, this, Duration, Timeout.InfiniteTimeSpan);
    }
}