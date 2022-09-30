using DiscordRPC;
using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class IdleSessionRenderer : IPlexSessionRenderer
{
    public RichPresence RenderSession(IPlexSession session)
    {
        return new RichPresence
        {
            State = "Idle",
        };
    }
}