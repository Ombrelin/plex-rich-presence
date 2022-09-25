using System;
using System.Threading.Tasks;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Avalonia.Services;

public class Clock : IClock
{
    public Task Delay(TimeSpan delay) => Task.Delay(delay);

    public DateTime Now => DateTime.Now;
}