using DiscordRPC;
using Microsoft.Extensions.Logging;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence;

public class DiscordService : IDiscordService
{
    private readonly ILogger logger;
    private readonly DiscordRpcClient discordRpcClient;
    private readonly PlexSessionRenderingService plexSessionRenderingService;

    public DiscordService(ILogger logger, PlexSessionRenderingService plexSessionRenderingService)
    {
        this.logger = logger;
        this.plexSessionRenderingService = plexSessionRenderingService;
        this.discordRpcClient = new DiscordRpcClient(applicationID: "698954724019273770");
        discordRpcClient.OnError += (sender, args) => this.logger.LogError(args.Message);
        discordRpcClient.Initialize();
    }

    public void SetDiscordPresenceToPlexSession(IPlexSession session)
    {
        RichPresence richPresence = plexSessionRenderingService.RenderSession(session);
        richPresence.Assets = new Assets
        {
            LargeImageKey = "icon"
        };
        discordRpcClient.SetPresence(richPresence);
    }
}