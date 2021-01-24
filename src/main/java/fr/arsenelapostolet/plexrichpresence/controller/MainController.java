package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.layout.StackPane;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;
import net.rgielen.fxweaver.core.FxmlView;
import org.apache.commons.lang3.StringUtils;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.io.OutputStream;
import java.util.Objects;


@Component
@FxmlView
public class MainController {

    private MainViewModel viewModel;

    public MainController(MainViewModel viewModel) {
        this.viewModel = viewModel;
    }

    @FXML
    private VBox vbox_login;

    @FXML
    private VBox vbox_status;

    @FXML
    private Button btn_login;

    @FXML
    private Button btn_logout;

    @FXML
    private Button btn_showLog;

    @FXML
    private Label lbl_plexStatus;

    @FXML
    private Label lbl_discordStatus;

    @FXML
    private CheckBox chk_rememberMe;

    private TextArea eventLog;

    Stage logWindow;

    OutputStream os;

    @FXML
    public void initialize() {
        eventLog = new TextArea();
        eventLog.setEditable(false);
        os = new TextAreaOutputStream(eventLog);
        MyStaticOutputStreamAppender.setStaticOutputStream(os);
        // Databinding
        this.chk_rememberMe.selectedProperty().bindBidirectional(this.viewModel.rememberMeProperty());
        this.lbl_plexStatus.textProperty().bindBidirectional(this.viewModel.plexStatusLabel());
        this.lbl_discordStatus.textProperty().bindBidirectional(this.viewModel.discordStatusLabel());
        this.btn_logout.disableProperty().bindBidirectional(this.viewModel.logoutButtonEnabled());
        this.viewModel.loadingProperty().addListener((observable, oldValue, newValue) -> {
            this.vbox_login.setManaged(!newValue);
            this.vbox_login.setVisible(!newValue);
            this.vbox_status.setManaged(newValue);
            this.vbox_status.setVisible(newValue);
        });

        if (!StringUtils.isEmpty(ConfigManager.getConfig("plex.token"))) {
            viewModel.setAuthToken(ConfigManager.getConfig("plex.token"));
            this.viewModel.login();
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

    @FXML
    public void openLog(ActionEvent event) {
        if (!Objects.isNull(logWindow)) {
            logWindow.show();
            logWindow.toFront();
            return;
        }
        StackPane layout = new StackPane();
        layout.getChildren().add(eventLog);
        Scene logScene = new Scene(layout, 400,200);
        logWindow = new Stage();
        logWindow.setTitle("Log");
        logWindow.setScene(logScene);
        logWindow.show();
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


