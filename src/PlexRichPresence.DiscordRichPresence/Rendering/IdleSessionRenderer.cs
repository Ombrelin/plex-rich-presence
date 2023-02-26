using DiscordRPC;
using PlexRichPresence.Core;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class IdleSessionRenderer : IPlexSessionRenderer
{
    public RichPresence RenderSession(PlexSession session)
    {
        return new RichPresence
        {
            State = "Idle",
        };
    }
}