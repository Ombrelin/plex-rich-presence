package fr.arsenelapostolet.plexrichpresence.controller;

import com.sun.tools.javac.comp.Check;
import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.User;
import fr.arsenelapostolet.plexrichpresence.services.RichPresence;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApi;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.WorkerService;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.*;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.control.TextArea;
import javafx.scene.control.TextField;
import javafx.scene.control.CheckBox;
import javafx.scene.layout.Pane;
import net.rgielen.fxweaver.core.FxmlView;
import org.apache.commons.lang3.StringUtils;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.env.Environment;
import org.springframework.stereotype.Component;

import java.sql.Time;
import java.util.concurrent.TimeUnit;


@Component
@FxmlView
public class MainController {

    private RichPresence richPresence;
    private PlexApi plexApi;

    @Autowired
    private Environment env;

    public MainController(RichPresence richPresence, PlexApi plexApi) {
        this.richPresence = richPresence;
        this.plexApi = plexApi;
    }

    @FXML
    private Label loginStatus;

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

    @FXML
    public void initialize(){
        this.login.applyCss();
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


        this.eventLog.appendText("Logging in as " + this.login.getText() + "...\n");

        plexApi.setCredentials(this.login.getText(), this.password.getText());
        plexApi.getServer(server -> {
            this.eventLog.appendText("Logged in as " + this.login.getText() + "\n");
            this.eventLog.appendText("Detected server : " + server.getName() + "\n");
            this.plexApi.getToken(this::fetchToken);
        }, exception -> {
            this.eventLog.appendText("Authentication failed : " + exception.getMessage() + "\n");
            this.credentialsPrompt.setManaged(true);
            this.credentialsPrompt.setVisible(true);
            this.loader.setVisible(false);
            this.loader.setManaged(false);
            this.login.clear();
            this.password.clear();
        });
    }

    public void fetchToken(User user) {
        this.eventLog.appendText("Successfully acquired user token for : " + user.getUsername() + "\n");
        this.loader.setVisible(false);
        this.loader.setManaged(false);
        this.plexApi.getSessions(this::fetchSession);
    }

    public void fetchSession(Metadatum userMetaDatum) {

        long currentTime = System.currentTimeMillis() / 1000;

        if (userMetaDatum == null) {
            this.eventLog.appendText("Nothing is playing.\n");
            richPresence.updateMessage(
                    "Nothing is playing",
                    ""
            );
            richPresence.setEndTimestamp(currentTime);
            waitBetweenCalls();
            return;
        };

        this.eventLog.appendText(
                "Found session for current user : "
                        + userMetaDatum.getTitle()
                        + "(" + userMetaDatum.getParentTitle() + ") from "
                        + userMetaDatum.getGrandparentTitle() + "\n"
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
}
