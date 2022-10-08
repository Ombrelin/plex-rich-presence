using DiscordRPC;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class SerieSessionRenderer : GenericSessionRenderer
{
    public override RichPresence RenderSession(IPlexSession session)
    {
        (string playerState, DateTime endTimeStamp) = this.RenderPlayerState(session);
        return new RichPresence
        {
            Details = $"⏏ {session.MediaTitle}",
            State = $"{playerState} {session.MediaGrandParentTitle}",
            Timestamps = new Timestamps
            {
                End = endTimeStamp
            }
        };
    }

    public SerieSessionRenderer(IClock clock) : base(clock)
    {
    }
}