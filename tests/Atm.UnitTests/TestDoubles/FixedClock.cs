namespace Atm.UnitTests.TestDoubles;

/// <summary>
/// A <see cref="TimeProvider"/> that always reports the same instant, so tests can
/// assert on transaction timestamps without touching the wall clock.
/// </summary>
internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow() => now;
}
