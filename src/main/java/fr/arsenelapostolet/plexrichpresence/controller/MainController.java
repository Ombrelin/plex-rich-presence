package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainer;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexServerService;
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

    private MediaContainer server;

    @FXML
    public void login(ActionEvent event) {
        this.eventLog.appendText("Logging in as " + this.login.getText() + "...\n");
        PlexServerService serverService = new PlexServerService(this.login.getText(), this.password.getText());
        serverService.setOnSucceeded(state -> {
            this.eventLog.appendText("Logged in as " + this.login.getText() + "\n");
            MediaContainer mediaContainer = serverService.getValue();
            System.out.println( mediaContainer);
            this.loader.setVisible(false);
            this.loader.setVisible(false);
            System.out.println(mediaContainer);
            this.eventLog.appendText("Detected server : " + mediaContainer.getServer().getName());
        });
        serverService.setOnFailed(state -> {
            this.eventLog.appendText("Authentication failed");
            this.credentialsPrompt.setManaged(true);
            this.credentialsPrompt.setVisible(true);
        });
        this.loader.setVisible(true);
        this.loader.setVisible(true);
        this.credentialsPrompt.setManaged(false);
        this.credentialsPrompt.setVisible(false);
        serverService.start();
    }

}
