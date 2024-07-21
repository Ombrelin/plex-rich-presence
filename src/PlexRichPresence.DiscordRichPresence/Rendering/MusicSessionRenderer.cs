using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MusicSessionRenderer : GenericSessionRenderer
{
    public override RichPresence RenderSession(PlexSession session)
    {
        (var playerState, var endTimeStamp) = RenderPlayerState(session);
        return new RichPresence
        {
            Details = $"♫ {session.MediaTitle}",
            State = $"{playerState} {session.MediaGrandParentTitle}",
            Timestamps = new Timestamps
            {
                End = endTimeStamp
            },
            Assets = new Assets()
            {
                LargeImageKey = session.Thumbnail
            }
        };
    }

    public MusicSessionRenderer(IClock clock) : base(clock)
    {
    }
}