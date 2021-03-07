package fr.arsenelapostolet.plexrichpresence.services;

import club.minnced.discord.rpc.DiscordEventHandlers;
import club.minnced.discord.rpc.DiscordRPC;
import club.minnced.discord.rpc.DiscordRichPresence;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import org.springframework.stereotype.Service;

@Service
public class RichPresence {
    private static final String token = "698954724019273770";
    private final DiscordRichPresence presence = new DiscordRichPresence();
    private final DiscordRPC lib = DiscordRPC.INSTANCE;
    private final DiscordEventHandlers handlers = new DiscordEventHandlers();
    private final String steamId = "";

    public RichPresence() {

        new Thread(() -> {
            while (!Thread.currentThread().isInterrupted()) {
                lib.Discord_RunCallbacks();
                try {
                    Thread.sleep(2000);
                } catch (InterruptedException ignored) {
                }
            }
        }, "RPC-Callback-Handler").start();

        presence.largeImageKey = "icon";

    }

    public void initHandlers() {
        lib.Discord_Initialize(token, handlers, true, steamId);
    }

    public DiscordEventHandlers getHandlers() {
        return handlers;
    }

    public void updateRichPresence(Metadatum session) {
        long currentTime = System.currentTimeMillis() / 1000;
        final String currentPlayerState;
        switch (session.getPlayer().getState()) {
            case "buffering":
                currentPlayerState = "⟲";
                this.setEndTimestamp(currentTime);
                break;
            case "paused":
                currentPlayerState = "⏸";
                this.setEndTimestamp(currentTime);
                break;
            default:
                currentPlayerState = "▶";
                this.setEndTimestamp(currentTime + ((Long.parseLong(session.getDuration()) - Long.parseLong(session.getViewOffset())) / 1000));
                break;
        }

        switch (session.getType()) {
            case "movie":
                this.updateMessage(currentPlayerState + " ", session.getTitle());
                break;
            case "episode":
                this.updateMessage(String.format("%s %s", currentPlayerState, session.getGrandparentTitle()), String.format("⏏ %02dx%02d - %s", Integer.parseInt(session.getParentIndex()), Integer.parseInt(session.getIndex()), session.getTitle()));
                break;
            case "track":
                this.updateMessage(String.format("%s %s", currentPlayerState, session.getGrandparentTitle()), String.format("♫ %02dx%02d - %s", Integer.parseInt(session.getParentIndex()), Integer.parseInt(session.getIndex()), session.getTitle()));
                break;
            default:
                this.updateMessage(
                        session.getGrandparentTitle() + " - " + session.getParentTitle(),
                        session.getTitle()
                );
                break;
        }
    }

    public void updateMessage(String state, String media) {
        presence.details = state;
        presence.state = media;
        lib.Discord_UpdatePresence(presence);
    }

    public void stopPresence() {
        lib.Discord_ClearPresence();
    }

    public void setEndTimestamp(long end) {
        presence.endTimestamp = end;
        lib.Discord_UpdatePresence(presence);
    }
}
