package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainer;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexServerService;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexTokenService;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.*;
import javafx.scene.layout.Pane;
import net.rgielen.fxweaver.core.FxmlView;
import org.springframework.stereotype.Component;

@Component
@FxmlView
public class MainController {

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
            MediaContainer mediaContainer = serverService.getValue();
            this.eventLog.appendText("Detected server : " + mediaContainer.getServer().getName() + "\n");
            this.server = mediaContainer.getServer();
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
            this.eventLog.appendText("Successfully acquired user token");
            this.user = login.getUser();
            this.loader.setVisible(false);
            this.loader.setManaged(false);
        });
        tokenService.start();
    }

    public void fetchSession() {

    }
}
