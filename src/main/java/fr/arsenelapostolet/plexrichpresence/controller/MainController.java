package fr.arsenelapostolet.plexrichpresence.controller;

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
import org.springframework.stereotype.Component;

@Component
@FxmlView
public class MainController {

    private RichPresence richPresence;
    private PlexApi plexApi;

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
    public void initialize(){
        this.login.applyCss();
    }

    @FXML
    public void login(ActionEvent event) {
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

        this.eventLog.appendText(
                "Found session for current user : "
                        + userMetaDatum.getTitle()
                        + "(" + userMetaDatum.getParentTitle() + ") from "
                        + userMetaDatum.getGrandparentTitle() + "\n"
        );

        richPresence.updateMessage(
                userMetaDatum.getParentTitle(),
                userMetaDatum.getTitle()
        );

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
