package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.Constants;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.image.Image;
import javafx.scene.layout.StackPane;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;
import jfxtras.styles.jmetro.JMetro;
import jfxtras.styles.jmetro.Style;
import net.rgielen.fxweaver.core.FxmlView;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.io.OutputStream;
import java.util.Objects;


@Component
@FxmlView
public class MainController {

    private final MainViewModel viewModel;

    public MainController(MainViewModel viewModel) {
        this.viewModel = viewModel;
    }

    @FXML
    private VBox vbox_login;

    @FXML
    private VBox vbox_status;

    @FXML
    private Button btn_logout;

    @FXML
    private Label lbl_plexStatus;

    @FXML
    private Label lbl_discordStatus;

    @FXML
    private CheckBox chk_rememberMe;

    private ListView<String> eventLog;

    private Stage logWindow;

    @FXML
    public void initialize() {
        eventLog = new ListView<>();
        eventLog.setItems(Constants.logList);

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
        final StackPane layout = new StackPane();
        layout.getChildren().add(eventLog);

        final Scene logScene = new Scene(layout, 400, 200);

        final JMetro jMetro = new JMetro(Style.DARK);
        jMetro.setScene(logScene);
        logScene.getStylesheets().add(getClass().getClassLoader().getResource("style.css").toExternalForm());
        logScene.getStylesheets().add(getClass().getClassLoader().getResource("theme.css").toExternalForm());

        logWindow = new Stage();
        logWindow.setTitle("Log");
        logWindow.setScene(logScene);
        logWindow.getIcons().add(new Image(getClass().getClassLoader().getResourceAsStream("images/icon.png")));
        logWindow.setTitle("Plex Rich Presence Logs");
        logWindow.show();
    }

}


