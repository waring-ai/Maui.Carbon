namespace Maui.Carbon.Relay.Services;

/// <summary>The current time; fixed in tests.</summary>
public interface IClock
{
    DateTime Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}

public sealed class FixedClock(DateTime now) : IClock
{
    public DateTime Now { get; set; } = now;
}

/// <summary>
/// Simulated network latency for the mock backend, so loading states (skeletons, inline loading) are visible.
/// <see cref="Scale"/> 0 makes every call immediate (tests, screenshots).
/// </summary>
public sealed class MockLatency
{
    public double Scale { get; set; } = 1;

    public Task WaitAsync(int milliseconds, CancellationToken ct = default) =>
        Scale <= 0 ? Task.CompletedTask : Task.Delay(TimeSpan.FromMilliseconds(milliseconds * Scale), ct);

    public static MockLatency None => new() { Scale = 0 };
}
