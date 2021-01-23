package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.*;
import javafx.scene.layout.Pane;
import net.rgielen.fxweaver.core.FxmlView;
import org.apache.commons.lang3.StringUtils;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.io.OutputStream;


@Component
@FxmlView
public class MainController {

    private MainViewModel viewModel;

    public MainController(MainViewModel viewModel) {
        this.viewModel = viewModel;
    }

    @FXML
    private Pane credentialsPrompt;

    @FXML
    private Button submitLogin;

    @FXML
    private ProgressIndicator loader;

    @FXML
    private TextArea eventLog;

    @FXML
    private CheckBox rememberMe;

    @FXML
    private Button logoutBtn;

    private OutputStream os;


    @FXML
    public void initialize() {
        os = new TextAreaOutputStream(eventLog);
        MyStaticOutputStreamAppender.setStaticOutputStream(os);
        // Databinding
        this.rememberMe.selectedProperty().bindBidirectional(this.viewModel.rememberMeProperty());
        this.viewModel.loadingProperty().addListener((observable, oldValue, newValue) -> {
            this.credentialsPrompt.setManaged(!newValue);
            this.credentialsPrompt.setVisible(!newValue);
            this.logoutBtn.setManaged(newValue);
            this.logoutBtn.setVisible(newValue);
            this.loader.setVisible(newValue);
            this.loader.setManaged(newValue);
        });
        this.loader.progressProperty().bind(this.viewModel.progressProperty());

        if (!StringUtils.isEmpty(ConfigManager.getConfig("plex.token"))) {
            viewModel.setAuthToken(ConfigManager.getConfig("plex.token"));
            submitLogin.fire();
        }


    }


    @FXML
    public void login(ActionEvent event) {
         this.viewModel.login();
    }

    @FXML
    public void logout(ActionEvent event) {
        this.viewModel.logout();
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


