using DiscordRPC;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MusicSessionRenderer : GenericSessionRenderer
{
    public override RichPresence RenderSession(IPlexSession session)
    {
        (string playerState, DateTime endTimeStamp) = this.RenderPlayerState(session);
        return new RichPresence
        {
            Details = $"♫ {session.MediaTitle}",
            State = $"{playerState} {session.MediaGrandParentTitle}",
            Timestamps = new Timestamps
            {
                End = endTimeStamp
            }
        };
    }

    public MusicSessionRenderer(IClock clock) : base(clock)
    {
    }
}