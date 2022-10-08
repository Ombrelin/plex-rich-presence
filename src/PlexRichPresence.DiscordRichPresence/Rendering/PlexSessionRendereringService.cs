using DiscordRPC;
using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class PlexSessionRenderingService
{
    private readonly PlexSessionRendererFactory rendererFactory;

    public PlexSessionRenderingService(PlexSessionRendererFactory rendererFactory)
    {
        this.rendererFactory = rendererFactory;
    }

    public RichPresence RenderSession(IPlexSession session)
    {
        return rendererFactory
            .BuildRendererForSession(session)
            .RenderSession(session);
    }
}