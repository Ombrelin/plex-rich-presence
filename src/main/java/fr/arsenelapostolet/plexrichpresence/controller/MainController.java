package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.services.RichPresence;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApi;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.WorkerService;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.*;
import javafx.scene.layout.Pane;
import net.rgielen.fxweaver.core.FxmlView;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.io.OutputStream;
import java.util.List;
import java.util.stream.Collectors;


@Component
@FxmlView
public class MainController {

    private Logger LOG = LoggerFactory.getLogger(MainController.class);
    private RichPresence richPresence;
    private PlexApi plexApi;

    public MainController(RichPresence richPresence, PlexApi plexApi) {
        this.richPresence = richPresence;
        this.plexApi = plexApi;
    }

    @FXML
    private Pane credentialsPrompt;

    @FXML
    private TextField login;

    @FXML
    private PasswordField password;

    @FXML
    private Button submitLogin;

    @FXML
    private ProgressIndicator loader;

    @FXML
    private TextArea eventLog;

    @FXML
    private CheckBox rememberMe;

    OutputStream os;

    private List<Server> servers;
    private String loggedUsername;

    @FXML
    public void initialize() {
        this.login.applyCss();
        os = new TextAreaOutputStream(eventLog);
        MyStaticOutputStreamAppender.setStaticOutputStream(os);
        if (!StringUtils.isEmpty(ConfigManager.getConfig("plex.username")) && (!StringUtils.isEmpty(ConfigManager.getConfig("plex.password")))) {
            this.login.setText(ConfigManager.getConfig("plex.username"));
            this.password.setText(ConfigManager.getConfig("plex.password"));
            submitLogin.fire();
        }

    }

    @FXML
    public void login(ActionEvent event) {

        if (this.rememberMe.isSelected()) {
            ConfigManager.setConfig("plex.username", this.login.getText());
            ConfigManager.setConfig("plex.password", this.password.getText());
        }

        this.credentialsPrompt.setManaged(false);
        this.credentialsPrompt.setVisible(false);
        this.loader.setVisible(true);
        this.loader.setManaged(true);


        LOG.info("Logging in as " + this.login.getText() + "...");

        List<Server> servers = plexApi
                .getServers(this.login.getText(), this.password.getText())
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
                .getToken(this.login.getText(), this.password.getText())
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

    private void fetchSession() {
        List<Metadatum> metadata = plexApi.getSessions(servers, this.loggedUsername)
                .doOnError(throwable -> {
                    handleError("Get session", throwable.getMessage());
                })
                .toBlocking()
                .first();
        this.processSessions(metadata);
    }

    private void handleError(String name, String message) {
        LOG.info(name + "failed : " + message);
        this.credentialsPrompt.setManaged(true);
        this.credentialsPrompt.setVisible(true);
        this.loader.setVisible(false);
        this.loader.setManaged(false);
        this.login.clear();
        this.password.clear();
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

    void waitBetweenCalls() {
        WorkerService workerService = new WorkerService();
        this.loader.setManaged(true);
        this.loader.setVisible(true);
        this.loader.progressProperty().bind(workerService.progressProperty());
        workerService.setOnSucceeded(state -> {
            this.fetchSession();
        });
        workerService.start();
    }


    private static class TextAreaOutputStream extends OutputStream {
        private TextArea textArea;

        public TextAreaOutputStream(TextArea textArea) {
            this.textArea = textArea;
        }

        @Override
        public void write(int b) throws IOException {
            textArea.appendText(String.valueOf((char) b));
        }
    }

}


