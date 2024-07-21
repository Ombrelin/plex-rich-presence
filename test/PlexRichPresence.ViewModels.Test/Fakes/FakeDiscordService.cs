using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeDiscordService : IDiscordService
{
    public List<PlexSession> Sessions { get; } = new();

    public void SetDiscordPresenceToPlexSession(PlexSession session) => Sessions.Add(session);

    public void StopRichPresence() { }
}