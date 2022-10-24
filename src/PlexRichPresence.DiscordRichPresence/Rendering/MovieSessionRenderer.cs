using DiscordRPC;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MovieSessionRenderer : GenericSessionRenderer
{
    public override RichPresence RenderSession(IPlexSession session)
    {
        (string playerState, DateTime endTimeStamp) = this.RenderPlayerState(session);
        return new RichPresence
        {
            Details = session.MediaTitle,
            State = playerState.Length < 2 ? playerState + '\x2800' : playerState,
            Timestamps = new Timestamps
            {
                End = endTimeStamp
            }
        };
    }

    public MovieSessionRenderer(IClock clock) : base(clock)
    {
    }
}