using DiscordRPC;
using Microsoft.Extensions.Logging;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence;

public class DiscordService : IDiscordService
{
    private readonly ILogger<DiscordService> logger;
    private DiscordRpcClient? discordRpcClient;
    private readonly PlexSessionRenderingService plexSessionRenderingService;

    public DiscordService(ILogger<DiscordService> logger, PlexSessionRenderingService plexSessionRenderingService)
    {
        this.logger = logger;
        this.plexSessionRenderingService = plexSessionRenderingService;
        this.discordRpcClient = CreateRpcClient();
    }

    private DiscordRpcClient CreateRpcClient()
    {
        var rpcClient = new DiscordRpcClient(applicationID: "698954724019273770");
        rpcClient.OnError += (sender, args) => this.logger.LogError(args.Message);
        rpcClient.Initialize();

        return rpcClient;
    }

    public void SetDiscordPresenceToPlexSession(IPlexSession session)
    {
        RichPresence richPresence = plexSessionRenderingService.RenderSession(session);
        richPresence.Assets = new Assets
        {
            LargeImageKey = "icon"
        };
        discordRpcClient ??= CreateRpcClient();
        discordRpcClient.SetPresence(richPresence);
    }

    public void StopRichPresence()
    {
        discordRpcClient?.Deinitialize();
        discordRpcClient = null;
    }
}