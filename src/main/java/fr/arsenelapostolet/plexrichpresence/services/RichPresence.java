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
    private final Logger LOG = LoggerFactory.getLogger(RichPresence.class);
    private static final String token = "698954724019273770";
    private DiscordRichPresence presence = new DiscordRichPresence();
    private DiscordRPC lib = DiscordRPC.INSTANCE;
    DiscordEventHandlers handlers;
    String steamId = "";
    public MainViewModel viewModel;

    public RichPresence() {
        handlers = new DiscordEventHandlers();
        handlers.ready = (user) -> {
            LOG.info("Connected to Discord RPC");
            Platform.runLater(() -> viewModel.discordStatusLabel().set("Connected"));
        };
        handlers.disconnected = (err, err1) -> {
            LOG.warn("Disconnected from Discord RPC");
            Platform.runLater(() -> viewModel.discordStatusLabel().set("Disconnected"));
        };
        handlers.errored = (err1, err2) -> {
            LOG.error("Error occurred when connecting to discord RPC");
            Platform.runLater(() -> viewModel.discordStatusLabel().set("Disconnected"));
        };
        lib.Discord_Initialize(token, handlers, true, steamId);

        new Thread(() -> {
            while (!Thread.currentThread().isInterrupted()) {
                lib.Discord_RunCallbacks();
                try {
                    Thread.sleep(2000);
                } catch (InterruptedException ignored) {
                }
            }
        }, "RPC-Callback-Handler").start();

        presence.startTimestamp = System.currentTimeMillis() / 1000; // epoch second
        presence.details = "Idling";
        presence.largeImageKey = "icon";
        lib.Discord_UpdatePresence(presence);
    }

    public void updateMessage(String state, String media){
        presence.details = state;
        presence.state = media;
        lib.Discord_UpdatePresence(presence);
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
