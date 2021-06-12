package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.application.Platform;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.*;
import javafx.scene.layout.HBox;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;
import net.rgielen.fxweaver.core.FxmlView;
import org.apache.commons.lang3.StringUtils;
import org.springframework.stereotype.Component;


@Component
@FxmlView
public class MainController {

    private final MainViewModel viewModel;

    private Stage logViewWindow;

    public MainController(MainViewModel viewModel) {
        this.viewModel = viewModel;
    }

    @FXML
    private VBox vbox_login;

    @FXML
    private VBox vbox_status;

    @FXML
    private VBox vbox_settings;

    @FXML
    private Button btn_logout;

    @FXML
    private Label lbl_plexStatus;

    @FXML
    private Label lbl_discordStatus;

    @FXML
    private CheckBox chk_rememberMe;

    @FXML
    private CheckBox chk_manualServer;

    @FXML
    private HBox hbox_manualServerInput;

    @FXML
    private TextField txt_plexAddress;

    @FXML
    private TextField txt_plexPort;


    private ListView<String> eventLog;

    @FXML
    public void initialize() {
        if (!StringUtils.isEmpty(ConfigManager.getConfig("plex.address")) && !StringUtils.isEmpty(ConfigManager.getConfig("plex.port")) ) {
            this.viewModel.plexAddressProperty().set(ConfigManager.getConfig("plex.address"));
            this.viewModel.plexPortProperty().set(ConfigManager.getConfig("plex.port"));
            this.viewModel.manualServerProperty().set(true);
        }

        if (!StringUtils.isEmpty(ConfigManager.getConfig("plex.token"))) {
            this.viewModel.setAuthToken(ConfigManager.getConfig("plex.token"));
            this.viewModel.rememberMeProperty().set(true);
            Platform.runLater(this.viewModel::login);
        }

        eventLog = new ListView<>();
        eventLog.setItems(LogViewController.logList);

        // Databinding
        this.chk_rememberMe.selectedProperty().bindBidirectional(this.viewModel.rememberMeProperty());
        this.lbl_plexStatus.textProperty().bindBidirectional(this.viewModel.plexStatusLabel());
        this.lbl_discordStatus.textProperty().bindBidirectional(this.viewModel.discordStatusLabel());
        this.btn_logout.disableProperty().bindBidirectional(this.viewModel.logoutButtonEnabled());
        this.txt_plexAddress.textProperty().bindBidirectional(this.viewModel.plexAddressProperty());
        this.txt_plexPort.textProperty().bindBidirectional(this.viewModel.plexPortProperty());
        this.vbox_settings.visibleProperty().bindBidirectional(this.viewModel.settingsShownProperty());

        this.viewModel.settingsShownProperty().addListener((observable, oldValue, newValue)  -> {
            this.vbox_login.setVisible(!newValue);
        });

        this.viewModel.loadingProperty().addListener((observable, oldValue, newValue) -> {
            this.vbox_login.setManaged(!newValue);
            this.vbox_login.setVisible(!newValue);
            this.vbox_status.setManaged(newValue);
            this.vbox_status.setVisible(newValue);
        });
        this.chk_manualServer.selectedProperty().addListener((observable, oldValue, newValue) -> {
            this.hbox_manualServerInput.setDisable(!newValue);
            if (!newValue) {
                this.viewModel.plexAddressProperty().set("");
                this.viewModel.plexPortProperty().set("");
            }
        });
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
        logViewWindow.show();
    }

    public void setLogViewStage(Stage logViewStage) {
        this.logViewWindow = logViewStage;
    }

    public void showSettings(ActionEvent actionEvent) {
        this.viewModel.showSettings();
    }

    public void closeSettings(ActionEvent actionEvent) {
        this.viewModel.closeSettings();
    }

    public void quit(ActionEvent actionEvent) {
        this.viewModel.quit();
    }
}


