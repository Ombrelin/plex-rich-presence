using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class SerieSessionRenderer : GenericSessionRenderer
{
    public override RichPresence RenderSession(PlexSession session)
    {
        (string playerState, DateTime startTimeStamp, DateTime endTimeStamp) = RenderPlayerState(session);

        var presence = new RichPresence
        {
            Type = ActivityType.Watching,
            StatusDisplay = StatusDisplayType.Details,
            Details = $"{session.MediaTitle}",
            State = $"{playerState} {session.MediaGrandParentTitle}",
            Assets = new Assets()
            {
                LargeImageKey = session.Thumbnail
            }
        };

        switch (session.PlayerState)
        {
            case ViewModels.Models.PlexPlayerState.Buffering:
            case ViewModels.Models.PlexPlayerState.Paused:
                // Add small image icons here for paused / loading
                break;

            case ViewModels.Models.PlexPlayerState.Playing:
                presence.Timestamps = new Timestamps
                {
                    Start = startTimeStamp,
                    End = endTimeStamp
                };
                break;

            case ViewModels.Models.PlexPlayerState.Idle:
                break;

            default:
                break;
        }

        return presence;
    }

    public SerieSessionRenderer(IClock clock) : base(clock)
    {
    }
}