package fr.arsenelapostolet.plexrichpresence.viewmodel;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.services.RichPresence;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApi;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.WorkerService;
import javafx.beans.property.*;
import javafx.event.ActionEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import sun.java2d.pipe.SpanShapeRenderer;

import java.util.List;
import java.util.stream.Collectors;

@Component
public class MainViewModel {

    private final Logger LOG = LoggerFactory.getLogger(MainViewModel.class);

    // Services
    private final RichPresence richPresence;
    private final PlexApi plexApi;
    private WorkerService workerService;

    // Properties
    private final StringProperty login = new SimpleStringProperty("");
    private final StringProperty password= new SimpleStringProperty("");
    private final BooleanProperty rememberMe = new SimpleBooleanProperty(true);
    private final BooleanProperty loading = new SimpleBooleanProperty(false);
    private final DoubleProperty progress = new SimpleDoubleProperty(0);

    private List<Server> servers;
    private String loggedUsername;


    public MainViewModel(RichPresence richPresence, PlexApi plexApi) {
        this.richPresence = richPresence;
        this.plexApi = plexApi;
    }

    public void login() {

        if (this.rememberMe.get()) {
            ConfigManager.setConfig("plex.username", this.login.get());
            ConfigManager.setConfig("plex.password", this.password.get());
        }

        this.loading.set(true);

        LOG.info("Logging in as " + this.login.get() + "...");

        List<Server> servers = plexApi
                .getServers(this.login.get(), this.password.get())
                .doOnError(throwable -> {
                    handleError("Fetch Servers", throwable.getMessage());
                }).toBlocking()
                .first()
                .stream()
                .filter(server -> server.getOwned().equals("1"))
                .collect(Collectors.toList());

        for (Server s : servers) {
            LOG.info("Fetched Server : " + s.getName());
        }

        PlexLogin user = plexApi
                .getToken(this.login.get(), this.password.get())
                .doOnError(throwable -> {
                    handleError("Get Token", throwable.getMessage());
                }).toBlocking().first();
        LOG.info("Successfully logged as : " + user.getUser().getUsername());

        for (Server s : servers) {
            s.setAccessToken(user.getUser().getAuthToken());
        }
        this.servers = servers;
        this.loggedUsername = user.getUser().getUsername();
        this.fetchSession();
    }

    private void handleError(String name, String message) {
        LOG.info(name + "failed : " + message);
        this.loading.set(false);
        this.login.set("");
        this.password.set("");
    }

    private void fetchSession() {
        List<Metadatum> metadata = plexApi.getSessions(servers, this.loggedUsername)
                .doOnError(throwable -> {
                    handleError("Get session", throwable.getMessage());
                })
                .toBlocking()
                .first();
        this.processSessions(metadata);
    }



    public void processSessions(List<Metadatum> userMetaDatum) {

        long currentTime = System.currentTimeMillis() / 1000;

        if (userMetaDatum.size() == 0) {
            LOG.info("No active sessions found for current user.");
            richPresence.updateMessage(
                    "Nothing is playing",
                    ""
            );
            richPresence.setEndTimestamp(currentTime);
            waitBetweenCalls();
            return;
        }

        Metadatum session = userMetaDatum.get(0);

        LOG.info(
                "Found session for current user : "
                        + session.getTitle()
                        + "(" + session.getParentTitle() + ") from "
                        + session.getGrandparentTitle()
        );


        richPresence.setEndTimestamp(currentTime + ((Long.parseLong(session.getDuration()) - Long.parseLong(session.getViewOffset())) / 1000));

        switch (session.getType()) {
            case "movie":
                richPresence.updateMessage(session.getTitle(), "");
                break;
            case "episode":
                richPresence.updateMessage("Watching " + session.getGrandparentTitle(), session.getTitle() + " - " + session.getParentTitle());
            default:
                richPresence.updateMessage(
                        session.getGrandparentTitle() + " - " + session.getParentTitle(),
                        session.getTitle()
                );
                break;
        }


        waitBetweenCalls();

    }

    public WorkerService getWorkerService() {
        return workerService;
    }

    void waitBetweenCalls() {
        workerService = new WorkerService();
        this.progress.bind(this.workerService.progressProperty());
        workerService.setOnSucceeded(state -> {
            this.fetchSession();
        });
        workerService.start();
    }

    public double getProgress() {
        return progress.get();
    }

    public DoubleProperty progressProperty() {
        return progress;
    }

    public void setProgress(double progress) {
        this.progress.set(progress);
    }

    public String getLogin() {
        return login.get();
    }

    public StringProperty loginProperty() {
        return login;
    }

    public void setLogin(String login) {
        this.login.set(login);
    }

    public String getPassword() {
        return password.get();
    }

    public StringProperty passwordProperty() {
        return password;
    }

    public void setPassword(String password) {
        this.password.set(password);
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
}
