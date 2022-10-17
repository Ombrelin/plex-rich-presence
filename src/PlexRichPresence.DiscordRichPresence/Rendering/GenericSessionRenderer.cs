using DiscordRPC;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class GenericSessionRenderer : IPlexSessionRenderer
{
    private readonly IClock clock;

    public GenericSessionRenderer(IClock clock)
    {
        this.clock = clock;
    }

    public virtual RichPresence RenderSession(IPlexSession session)
    {
        (_, DateTime endTimeStamps) = RenderPlayerState(session);
        return new RichPresence
        {
            Details = session.MediaGrandParentTitle + " - " + session.MediaParentTitle,
            State = session.MediaTitle,
            Timestamps = new Timestamps
            {
                End = endTimeStamps
            }
        };
    }

    protected (string playerState, DateTime endTimeStamp) RenderPlayerState(IPlexSession session)
    {
        return session.PlayerState switch
        {
            PlexPlayerState.Buffering => ("⟲", this.clock.Now.ToUniversalTime()),
            PlexPlayerState.Paused => ("⏸", this.clock.Now.ToUniversalTime()),
            PlexPlayerState.Playing => ("▶", this.clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static long ComputeSessionRemainingTime(IPlexSession session) =>
        (session.Duration - session.ViewOffset) / 1000;
}