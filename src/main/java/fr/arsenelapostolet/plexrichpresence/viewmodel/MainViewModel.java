package fr.arsenelapostolet.plexrichpresence.viewmodel;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.Constants;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;
import fr.arsenelapostolet.plexrichpresence.services.RichPresence;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApi;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.WorkerService;
import javafx.application.Platform;
import javafx.beans.property.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import rx.schedulers.Schedulers;

import java.awt.*;
import java.net.URI;
import java.util.List;

@Component
public class MainViewModel {

    private final Logger LOG = LoggerFactory.getLogger(MainViewModel.class);

    // Services
    private final RichPresence richPresence;
    private final PlexApi plexApi;
    private WorkerService workerService;

    // Properties
    private final BooleanProperty rememberMe = new SimpleBooleanProperty(true);
    private final BooleanProperty loading = new SimpleBooleanProperty(false);
    private final DoubleProperty progress = new SimpleDoubleProperty(0);

    private final StringProperty plexStatusLabel = new SimpleStringProperty("...");
    private final StringProperty discordStatusLabel = new SimpleStringProperty("...");

    private List<Server> servers;
    private String loggedUsername;
    private String authToken;

    public MainViewModel(RichPresence richPresence, PlexApi plexApi) {
        this.richPresence = richPresence;
        richPresence.viewModel = this;
        this.plexApi = plexApi;
    }

    public void login() {
        if (!rememberMe.get()) {
            ConfigManager.setConfig("plex.token", "");
        }
        this.loading.set(true);
        this.plexStatusLabel.set("Logging in...");
        LOG.info("Logging in");

        if (authToken == null) {
            plexApi.getPlexAuthPin(true, Constants.plexProduct, Constants.plexClientIdentifer)
                    .doOnError(throwable -> handleError("Get plex auth pin ", throwable.getMessage()))
                    .subscribeOn(Schedulers.io())
                    .flatMap(response -> {
                        String authURL = String.format("https://app.plex.tv/auth#?clientID=%s&code=%s&context%%5Bdevice%%5D%%5Bproduct%%5D=%s",
                                Constants.plexClientIdentifer,
                                response.code,
                                Constants.plexProduct);
                        LOG.info("Please sign in using this url: " + authURL);
                        Desktop desktop = java.awt.Desktop.getDesktop();
                        try {
                            desktop.browse(new URI(authURL));
                        } catch (Exception e) {
                            handleError("Open login page ", e.getMessage());
                        }
                        Platform.runLater(() -> plexStatusLabel.set("Waiting for user to login..."));
                        return plexApi.validatePlexAuthPin(response.id, response.code, Constants.plexClientIdentifer)
                                .doOnError(throwable -> handleError("Validate auth pin/code ", throwable.getMessage()));
                    })
                    .flatMap(response -> {
                        LOG.info("Obtaining Plex servers...");
                        Platform.runLater(() -> plexStatusLabel.set("Obtaining plex servers..."));
                        this.authToken = response.authToken;
                        return plexApi.getServers(response.authToken).doOnError(throwable -> handleError("Obtain plex server ", throwable.getMessage()));
                    })
                    .flatMap(response -> {
                        LOG.info("Obtaining user info...");
                        Platform.runLater(() -> plexStatusLabel.set("Obtaining user info..."));
                        this.servers = response;
                        return plexApi.getUser(authToken).doOnError(throwable -> handleError("Obtain user info ", throwable.getMessage()));
                    })
                    .subscribe(this::postLogin, throwable -> handleError("Initialization ", throwable.getMessage()));
        } else {
            plexApi.getServers(authToken)
                    .subscribeOn(Schedulers.io())
                    .flatMap(response -> {
                        LOG.info("Obtaining Plex servers...");
                        Platform.runLater(() -> plexStatusLabel.set("Obtaining plex servers..."));
                        this.servers = response;
                        return plexApi.getUser(authToken).doOnError(throwable -> handleError("Obtain user info ", throwable.getMessage()));
                    })
                    .subscribe(this::postLogin, throwable -> handleError("Initialization ", throwable.getMessage()));
        }

    }

    private void postLogin(User response) {
        LOG.info("Successfully logged in as: " + response.getUsername());
        Platform.runLater(() -> plexStatusLabel.set("Logged in"));
        if (this.rememberMe.get()){
            ConfigManager.setConfig("plex.token", authToken);
        }
        this.loggedUsername = response.getUsername();
        this.fetchSession();
    }

    public void logout(){
        this.loading.set(false);
        workerService.cancel();
        authToken = null;
        LOG.info("Logged out");
    }

    private void handleError(String name, String message) {
        LOG.info(name + "failed : " + message);
        this.loading.set(false);
    }

    private void fetchSession() {
        plexApi.getSessions(servers, this.loggedUsername)
                .doOnError(throwable -> {
                    if (throwable instanceof NullPointerException) {
                        processSessions(null);
                    } else {
                        handleError("Error getting sessions ", throwable.getMessage());
                    }
                })
                .subscribe(this::processSessions);
    }



    public void processSessions(List<Metadatum> userMetaDatum) {
        long currentTime = System.currentTimeMillis() / 1000;


        if ((userMetaDatum == null) || (userMetaDatum.size() == 0)) {
            LOG.info("No active sessions found for current user.");
            Platform.runLater(() -> plexStatusLabel.set("Idling/No Streams"));
            richPresence.updateMessage(
                    "Idling",
                    ""
            );
            richPresence.setEndTimestamp(currentTime);
            waitBetweenCalls();
            return;
        }

        Metadatum session = userMetaDatum.get(0);

        LOG.info(
                "Session acquired for user : "
                        + session.getTitle()
                        + "(" + session.getParentTitle() + ") from "
                        + session.getGrandparentTitle()
        );
        Platform.runLater(() -> plexStatusLabel.set("Stream detected!"));


        String currentPlayerState;
        switch (session.getPlayer().getState()) {
            case "buffering":
                currentPlayerState = "Buffering";
                richPresence.setEndTimestamp(currentTime);
                break;
            case "paused":
                currentPlayerState = "Paused";
                richPresence.setEndTimestamp(currentTime);
                break;
            default:
                currentPlayerState = "Playing";
                richPresence.setEndTimestamp(currentTime + ((Long.parseLong(session.getDuration()) - Long.parseLong(session.getViewOffset())) / 1000));
                break;
        }

        switch (session.getType()) {
            case "movie":
                richPresence.updateMessage(currentPlayerState, session.getTitle());
                break;
            case "episode":
                richPresence.updateMessage(String.format("(%s) %s", currentPlayerState, session.getGrandparentTitle()), String.format("S%02dE%02d - %s", Integer.parseInt(session.getParentIndex()), Integer.parseInt(session.getIndex()), session.getTitle()) );
                break;
            default:
                richPresence.updateMessage(
                        session.getGrandparentTitle() + " - " + session.getParentTitle(),
                        session.getTitle()
                );
                break;
        }


        waitBetweenCalls();


    }

    public StringProperty plexStatusLabel() {
        return plexStatusLabel;
    }

    public StringProperty discordStatusLabel() {
        return discordStatusLabel;
    }

    public WorkerService getWorkerService() {
        return workerService;
    }

    void waitBetweenCalls() {
        workerService = new WorkerService();
        this.progress.bind(this.workerService.progressProperty());
        workerService.setOnSucceeded(state -> this.fetchSession());
        workerService.start();
    }

    public double getProgress() {
        return progress.get();
    }

    public void setAuthToken(String authToken) {
        this.authToken = authToken;
    }

    public DoubleProperty progressProperty() {
        return progress;
    }

    public void setProgress(double progress) {
        this.progress.set(progress);
    }

    public boolean isRememberMe() {
        return rememberMe.get();
    }

    public BooleanProperty rememberMeProperty() {
        return rememberMe;
    }

    public void setRememberMe(boolean rememberMe) {
        this.rememberMe.set(rememberMe);
    }

    public boolean isLoading() {
        return loading.get();
    }

    public BooleanProperty loadingProperty() {
        return loading;
    }

    public void setLoading(boolean loading) {
        this.loading.set(loading);
    }

    public void openLog() {

    }
}
