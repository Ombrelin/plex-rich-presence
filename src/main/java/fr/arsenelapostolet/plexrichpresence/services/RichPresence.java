package fr.arsenelapostolet.plexrichpresence.services;

import club.minnced.discord.rpc.DiscordEventHandlers;
import club.minnced.discord.rpc.DiscordRPC;
import club.minnced.discord.rpc.DiscordRichPresence;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.application.Platform;
import javafx.beans.property.StringProperty;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
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

    public void updateMessage(String state, String media){
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

    public void setStartTimestamp(long start) {
        presence.startTimestamp = start;
        lib.Discord_UpdatePresence(presence);
    }

}
