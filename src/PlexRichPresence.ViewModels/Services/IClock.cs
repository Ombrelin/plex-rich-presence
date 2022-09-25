namespace PlexRichPresence.ViewModels.Services;

public interface IClock
{
    Task Delay(TimeSpan delay);
    DateTime Now { get; }
}