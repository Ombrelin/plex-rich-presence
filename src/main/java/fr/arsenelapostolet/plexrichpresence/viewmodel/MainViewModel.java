package fr.arsenelapostolet.plexrichpresence.viewmodel;

import com.google.gson.Gson;
import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.SharedVariables;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.PlexAuth;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;
import fr.arsenelapostolet.plexrichpresence.services.RichPresence;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApi;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.WorkerService;
import javafx.application.Platform;
import javafx.beans.property.*;
import javafx.event.ActionEvent;
import javafx.scene.control.Alert;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import rx.Observable;
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
    private final BooleanProperty rememberMe = new SimpleBooleanProperty(false);
    private final BooleanProperty manualServer = new SimpleBooleanProperty(false);
    private final BooleanProperty loading = new SimpleBooleanProperty(false);
    private final DoubleProperty progress = new SimpleDoubleProperty(0);
    private final BooleanProperty logoutButtonDisabled = new SimpleBooleanProperty(false);

    private final StringProperty plexStatusLabel = new SimpleStringProperty("...");
    private final StringProperty discordStatusLabel = new SimpleStringProperty("...");

    private final StringProperty plexAddress = new SimpleStringProperty();
    private final StringProperty plexPort = new SimpleStringProperty();

    private final BooleanProperty settingsShown = new SimpleBooleanProperty(false);

    private List<Server> servers;
    private String loggedUsername;
    private String authToken;

    public MainViewModel(RichPresence richPresence, PlexApi plexApi) {
        this.richPresence = richPresence;
        this.plexApi = plexApi;



        this.richPresence.getHandlers().ready = (user) -> {
            LOG.info("Connected to Discord RPC");
            Platform.runLater(() -> discordStatusLabel().set("Connected"));
        };
        this.richPresence.getHandlers().disconnected = (err, err1) -> {
            LOG.warn("Disconnected from Discord RPC");
            Platform.runLater(() -> discordStatusLabel().set("Disconnected"));
        };
        this.richPresence.getHandlers().errored = (err1, err2) -> {
            LOG.error("Error occurred when connecting to discord RPC");
            LOG.error("Error Code: " + err1);
            LOG.error("Message: " + err2);
            Platform.runLater(() -> discordStatusLabel().set("Disconnected"));
        };
        this.richPresence.initHandlers();
    }

    public void login() {
        if (!rememberMe.get()) {
            ConfigManager.setConfig("plex.token", "");
            rememberMeProperty().set(false);
        }
        if (plexAddress.isEmpty().get() && plexPort.isEmpty().get()) {
            ConfigManager.setConfig("plex.address", "");
            ConfigManager.setConfig("plex.port", "");
        }
        this.logoutButtonDisabled.set(true);
        this.loading.set(true);
        this.plexStatusLabel.set("Logging in...");
        LOG.info("Logging in");

        if (authToken == null) {
            plexApi.getPlexAuthPin(true, SharedVariables.plexProduct, SharedVariables.plexClientIdentifier)
                    .doOnError(throwable -> handleError("Get plex auth pin ", throwable.getMessage()))
                    .subscribeOn(Schedulers.io())
                    .flatMap(response -> {
                        LOG.debug(new Gson().toJson(response));
                        String authURL = String.format("https://app.plex.tv/auth#?clientID=%s&code=%s&context%%5Bdevice%%5D%%5Bproduct%%5D=%s",
                                SharedVariables.plexClientIdentifier,
                                response.code,
                                SharedVariables.plexProduct);
                        LOG.info("Please sign in using this url: " + authURL);
                        Desktop desktop = Desktop.getDesktop();
                        try {
                            desktop.browse(new URI(authURL));
                        } catch (Exception e) {
                            handleError("Open login page ", e.getMessage());
                        }
                        Platform.runLater(() -> plexStatusLabel.set("Waiting for user to login..."));
                        return plexApi.validatePlexAuthPin(response.id, response.code, SharedVariables.plexClientIdentifier)
                                .doOnError(throwable -> handleError("Validate auth pin/code ", throwable.getMessage()));
                    })
                    .flatMap(response -> {
                        LOG.debug(new Gson().toJson(response));
                        LOG.info("Obtaining Plex servers...");
                        Platform.runLater(() -> plexStatusLabel.set("Obtaining plex servers..."));
                        this.authToken = response.authToken;
                        SharedVariables.authToken = response.authToken;
                        return checkServers(response);
                    })
                    .flatMap(response -> {
                        LOG.debug(new Gson().toJson(response));
                        LOG.info("Obtaining user info...");
                        Platform.runLater(() -> plexStatusLabel.set("Obtaining user info..."));
                        this.servers = response;
                        return plexApi.getUser(authToken).doOnError(throwable -> handleError("Obtain user info ", throwable.getMessage()));
                    })
                    .subscribe(this::postLogin, throwable -> handleError("Initialization ", throwable.getMessage()));
        } else {
            SharedVariables.authToken = authToken;
            checkServers(authToken)
                    .subscribeOn(Schedulers.io())
                    .flatMap(response -> {
                        LOG.debug(new Gson().toJson(response));
                        LOG.info("Obtaining Plex servers...");
                        Platform.runLater(() -> plexStatusLabel.set("Obtaining plex servers..."));
                        this.servers = response;
                        return plexApi.getUser(authToken).doOnError(throwable -> handleError("Obtain user info ", throwable.getMessage()));
                    }).doOnError(throwable -> handleError("Obtain plex server ", throwable.getMessage()))
                    .subscribe(this::postLogin, throwable -> handleError("Initialization ", throwable.getMessage()));
        }

    }

    private Observable<List<Server>> checkServers(PlexAuth response) {
        if (plexAddress.isNotEmpty().get() && plexPort.isNotEmpty().get()) {
            LOG.info(String.format("Manual plex server specified. Address: %s Port: %s", plexAddress.get(), plexPort.get()));
            return plexApi.getServers(response.authToken, plexAddress.get(), plexPort.get()).doOnError(throwable -> handleError("Obtain plex server ", throwable.getMessage()));
        } else {
            return plexApi.getServers(response.authToken).doOnError(throwable -> handleError("Obtain plex server ", throwable.getMessage()));
        }
    }

    private Observable<List<Server>> checkServers(String authToken) {
        if (plexAddress.isNotEmpty().get() && plexPort.isNotEmpty().get()) {
            LOG.info(String.format("Manual plex server specified. Address: %s Port: %s", plexAddress.get(), plexPort.get()));
            return plexApi.getServers(authToken, plexAddress.get(), plexPort.get()).doOnError(throwable -> handleError("Obtain plex server ", throwable.getMessage()));
        } else {
            return plexApi.getServers(authToken).doOnError(throwable -> handleError("Obtain plex server ", throwable.getMessage()));
        }
    }

    private void postLogin(User response) {
        if (this.servers.size() == 0) {
            handleError("Obtain plex server ", "Failed to find any plex servers.");
            return;
        }
        if (plexAddress.isNotEmpty().get() && plexPort.isNotEmpty().get()) {
            ConfigManager.setConfig("plex.address", plexAddress.get());
            ConfigManager.setConfig("plex.port", plexPort.get());
        }
        LOG.debug(new Gson().toJson(response));
        LOG.info("Successfully logged in as: " + response.getUsername());
        Platform.runLater(() -> {
            plexStatusLabel.set("Logged in");
            logoutButtonDisabled.set(false);
        });
        if (this.rememberMe.get()) {
            ConfigManager.setConfig("plex.token", authToken);
        }
        this.loggedUsername = response.getUsername();
        this.fetchSession();
    }

    public void logout() {
        this.loading.set(false);
        plexAddress.set("");
        plexPort.set("");
        workerService.cancel();
        authToken = null;
        richPresence.stopPresence();
        LOG.info("Logged out");
    }

    private void handleError(String name, String message) {
        this.authToken = null;
        LOG.error(name + "failed : " + message);
        this.loading.set(false);
        Platform.runLater(() -> {
            Alert alert = new Alert(Alert.AlertType.ERROR);
            alert.setTitle("Error");
            alert.setHeaderText(name);
            alert.setContentText(message);
            alert.showAndWait();
        });
    }

    private void fetchSession() {
        plexApi.getSessions(servers, this.loggedUsername)
                .subscribeOn(Schedulers.io())
                .doOnError(throwable -> {
                    if (throwable instanceof NullPointerException) {
                        processSessions(null);
                    } else {
                        handleError("Error getting sessions ", throwable.getMessage());
                    }
                })
                .subscribe(response -> {
                    LOG.debug(new Gson().toJson(response));
                    processSessions(response);
                });
    }


    public void processSessions(List<Metadatum> userMetaDatum) {

        if ((userMetaDatum == null) || (userMetaDatum.size() == 0)) {
            LOG.info("No active sessions found for current user.");
            Platform.runLater(() -> plexStatusLabel.set("No Stream Detected"));
            richPresence.stopPresence();
            waitBetweenCalls();
            return;
        }

        final Metadatum session = userMetaDatum.get(0);

        LOG.info(
                "Session acquired for user : "
                        + session.getTitle()
                        + "(" + session.getParentTitle() + ") from "
                        + session.getGrandparentTitle()
        );

        Platform.runLater(() -> plexStatusLabel.set("Stream Detected"));
        richPresence.updateRichPresence(session);
        waitBetweenCalls();
    }



    public StringProperty plexStatusLabel() {
        return plexStatusLabel;
    }
    public StringProperty discordStatusLabel() {
        return discordStatusLabel;
    }

    void waitBetweenCalls() {
        workerService = new WorkerService();
        this.progress.bind(this.workerService.progressProperty());
        workerService.setOnSucceeded(state -> this.fetchSession());
        workerService.start();
    }

    public void setAuthToken(String authToken) {
        this.authToken = authToken;
    }
    public BooleanProperty rememberMeProperty() {
        return rememberMe;
    }
    public BooleanProperty manualServerProperty() {
        return manualServer;
    }
    public BooleanProperty logoutButtonEnabled() {
        return logoutButtonDisabled;
    }
    public StringProperty plexAddressProperty() {
        return plexAddress;
    }
    public StringProperty plexPortProperty() {
        return plexPort;
    }
    public BooleanProperty loadingProperty() {
        return loading;
    }

    public void quit(){
        if (!this.rememberMeProperty().get()) {
            ConfigManager.setConfig("plex.token", "");
            this.rememberMeProperty().set(false);
        }
        ConfigManager.saveConfig();
        Platform.exit();
        System.exit(0);
    }

    public boolean isSettingsShown() {
        return settingsShown.get();
    }

    public BooleanProperty settingsShownProperty() {
        return settingsShown;
    }

    public void setSettingsShown(boolean settingsShown) {
        this.settingsShown.set(settingsShown);
    }

    public void showSettings() {
        this.settingsShown.set(true);
    }

    public void closeSettings() {
        this.settingsShown.set(false);
    }
}
