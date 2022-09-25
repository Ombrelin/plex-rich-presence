using System;
using System.Threading;
using System.Threading.Tasks;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity.Tests;

public class FakeClock : IClock
{
    public DateTime Now { get; }
    private TimeSpan accumulatedDelay = TimeSpan.Zero;

    public DateTime DateTimeAfterDelay => Now.Add(accumulatedDelay);
    
    public FakeClock(DateTime now)
    {
        this.Now = now;
    }


    public Task Delay(TimeSpan delay)
    {
        accumulatedDelay = accumulatedDelay.Add(delay);
        return Task.CompletedTask;
    }


}