namespace TaskBoard.Api.Tests;

// Each read of the clock moves it one second forward, so creation order is deterministic.
public sealed class SteppingTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow()
    {
        var now = _now;
        _now = _now.AddSeconds(1);
        return now;
    }
}
