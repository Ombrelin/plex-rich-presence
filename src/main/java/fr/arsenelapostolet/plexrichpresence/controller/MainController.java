package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.User;
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

    @FXML
    public void initialize(){
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

        plexApi.setCredentials(this.login.getText(), this.password.getText());
        plexApi.getServer(server -> {
            LOG.info("Logged in as " + this.login.getText());
            LOG.info("Detected server : " + server.getName());
            this.plexApi.getToken(this::fetchToken);
        }, exception -> {
            LOG.info("Authentication failed : " + exception.getMessage());
            this.credentialsPrompt.setManaged(true);
            this.credentialsPrompt.setVisible(true);
            this.loader.setVisible(false);
            this.loader.setManaged(false);
            this.login.clear();
            this.password.clear();
        });
    }

    public void fetchToken(User user) {
        LOG.info("Successfully acquired user token for : " + user.getUsername());
        this.loader.setVisible(false);
        this.loader.setManaged(false);
        this.plexApi.getSessions(this::fetchSession);
    }

    public void fetchSession(Metadatum userMetaDatum) {

        long currentTime = System.currentTimeMillis() / 1000;

        if (userMetaDatum == null) {
            LOG.info("No active sessions found for current user.");
            richPresence.updateMessage(
                    "Nothing is playing",
                    ""
            );
            richPresence.setEndTimestamp(currentTime);
            waitBetweenCalls();
            return;
        };

        LOG.info(
                "Found session for current user : "
                        + userMetaDatum.getTitle()
                        + "(" + userMetaDatum.getParentTitle() + ") from "
                        + userMetaDatum.getGrandparentTitle() 
        );



        richPresence.setEndTimestamp(currentTime + ((Long.parseLong(userMetaDatum.getDuration()) - Long.parseLong(userMetaDatum.getViewOffset())) / 1000));

        switch (userMetaDatum.getType()) {
            case "movie":
                richPresence.updateMessage(userMetaDatum.getTitle(), "");
                break;
            case "episode":
                richPresence.updateMessage("Watching " + userMetaDatum.getGrandparentTitle(), userMetaDatum.getTitle() + " - " + userMetaDatum.getParentTitle());
            default:
                richPresence.updateMessage(
                        userMetaDatum.getGrandparentTitle() + " - " + userMetaDatum.getParentTitle(),
                        userMetaDatum.getTitle()
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
            this.plexApi.getSessions(this::fetchSession);
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


