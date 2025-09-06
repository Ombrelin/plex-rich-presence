using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MovieSessionRenderer(IClock clock) : GenericSessionRenderer(clock)
{
    public override RichPresence RenderSession(PlexSession session)
    {
        DiscordPlayerState playerState = RenderPlayerState(session);
        
        return new RichPresence
        {
            Type = ActivityType.Watching,
            StatusDisplay = StatusDisplayType.Details,
            Details = session.MediaTitle,
            Assets = new Assets
            {
                SmallImageKey = playerState.SmallAssetImageKey,
                LargeImageKey = session.Thumbnail
            },
            Timestamps = playerState.Timestamps
        };
    }
}