using DiscordRPC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class PlexSessionRenderingService
{
    private readonly PlexSessionRendererFactory rendererFactory;
    private readonly ILogger<PlexSessionRenderingService> logger;

    public PlexSessionRenderingService(PlexSessionRendererFactory rendererFactory, ILogger<PlexSessionRenderingService> logger)
    {
        this.rendererFactory = rendererFactory;
        this.logger = logger;
    }

    public RichPresence RenderSession(IPlexSession session)
    {
        RichPresence renderedSession = rendererFactory
            .BuildRendererForSession(session)
            .RenderSession(session);
        
        this.logger.LogInformation("Rendered Plex Session : {Session}", JsonConvert.SerializeObject(renderedSession));
        
        return renderedSession;
    }
}