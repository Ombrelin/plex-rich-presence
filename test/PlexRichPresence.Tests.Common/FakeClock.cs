using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.Tests.Common;

public class FakeClock : IClock
{
    public DateTime Now { get; }
    private TimeSpan _accumulatedDelay = TimeSpan.Zero;

    public DateTime DateTimeAfterDelay => Now.Add(_accumulatedDelay);

    public FakeClock(DateTime now)
    {
        Now = now;
    }
    
    public Task Delay(TimeSpan delay)
    {
        _accumulatedDelay = _accumulatedDelay.Add(delay);
        return Task.CompletedTask;
    }
}