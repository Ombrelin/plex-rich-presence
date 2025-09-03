using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MovieSessionRenderer(IClock clock) : GenericSessionRenderer(clock)
{
    public override RichPresence RenderSession(PlexSession session)
    {
        var presence = new RichPresence
        {
            Type = ActivityType.Watching,
            StatusDisplay = StatusDisplayType.Details,
            Details = session.MediaTitle,
            Assets = new Assets()
            {
                LargeImageKey = session.Thumbnail
            }
        };

        RenderPlayerState(session, presence);

        return presence;
    }
}