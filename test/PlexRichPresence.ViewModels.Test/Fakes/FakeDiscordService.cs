using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeDiscordService : IDiscordService
{
    public List<IPlexSession> Sessions { get; } = new();
    
    public void SetDiscordPresenceToPlexSession(IPlexSession session)
    {
        Sessions.Add(session);
    }
}