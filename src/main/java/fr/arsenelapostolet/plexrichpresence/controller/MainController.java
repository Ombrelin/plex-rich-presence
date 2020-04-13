package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.model.*;
import fr.arsenelapostolet.plexrichpresence.services.RichPresence;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexServerService;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexServerSessionsService;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexTokenService;
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

    public MainController(RichPresence richPresence) {
        this.richPresence = richPresence;
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

    private Server server;
    private User user;

    @FXML
    public void login(ActionEvent event) {
        this.credentialsPrompt.setManaged(false);
        this.credentialsPrompt.setVisible(false);
        this.loader.setVisible(true);
        this.loader.setManaged(true);


        this.eventLog.appendText("Logging in as " + this.login.getText() + "...\n");
        PlexServerService serverService = new PlexServerService(this.login.getText(), this.password.getText());
        serverService.setOnSucceeded(state -> {
            this.eventLog.appendText("Logged in as " + this.login.getText() + "\n");
            MediaContainerServer mediaContainerServer = serverService.getValue();
            this.eventLog.appendText("Detected server : " + mediaContainerServer.getServer().getName() + "\n");
            this.server = mediaContainerServer.getServer();
            this.fetchToken();
        });
        serverService.setOnFailed(state -> {
            this.eventLog.appendText("Authentication failed : " + serverService.getException().getMessage() + "\n");
            this.credentialsPrompt.setManaged(true);
            this.credentialsPrompt.setVisible(true);
            this.loader.setVisible(false);
            this.loader.setManaged(false);
            this.login.clear();
            this.password.clear();

        });
        serverService.start();
    }

    public void fetchToken() {
        PlexTokenService tokenService = new PlexTokenService(this.login.getText(), this.password.getText());
        tokenService.setOnSucceeded(state -> {
            PlexLogin login = tokenService.getValue();
            this.eventLog.appendText("Successfully acquired user token\n");
            this.user = login.getUser();
            this.loader.setVisible(false);
            this.loader.setManaged(false);

            this.fetchSession();
        });
        tokenService.start();
    }

    public void fetchSession() {
        PlexServerSessionsService plexServerSessionsService = new PlexServerSessionsService(this.server.getAddress(), this.server.getPort(), this.user.getAuthToken());
        plexServerSessionsService.setOnSucceeded(state -> {
            PlexSessions plexSessions = plexServerSessionsService.getValue();
            try {
                Metadatum userMetaDatum = plexSessions.getMediaContainer().getMetadata().stream()
                        .filter(session -> session.getUser().getTitle().equals(user.getUsername()))
                        .findAny()
                        .orElseThrow(IllegalArgumentException::new);
                this.eventLog.appendText("Found session for current user : " + userMetaDatum.getTitle()
                        +"(" + userMetaDatum.getParentTitle() + ") from " + userMetaDatum.getGrandparentTitle() +"\n");

                richPresence.updateMessage(userMetaDatum.getParentTitle(),userMetaDatum.getTitle());
                waitBetweenCalls();
            } catch (IllegalArgumentException exception) {
                this.eventLog.appendText("Found no session for current user\n");
            }
        });
        plexServerSessionsService.setOnFailed(state -> {
            this.eventLog.appendText("Fetching sessions failed : " + plexServerSessionsService.getException().getMessage() + "\n");
        });

        plexServerSessionsService.start();
    }

    void waitBetweenCalls(){
        WorkerService workerService = new WorkerService();
        this.loader.setManaged(true);
        this.loader.setVisible(true);
        this.loader.progressProperty().bind(workerService.progressProperty());
        workerService.setOnSucceeded(state -> {
            fetchSession();
        });
        workerService.start();
    }
}
