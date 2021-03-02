package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.SharedVariables;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
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

    private Stage logWindow;

    @FXML
    public void initialize() {
        eventLog = new ListView<>();
        eventLog.setItems(SharedVariables.logList);

        // Databinding
        this.chk_rememberMe.selectedProperty().bindBidirectional(this.viewModel.rememberMeProperty());
        this.lbl_plexStatus.textProperty().bindBidirectional(this.viewModel.plexStatusLabel());
        this.lbl_discordStatus.textProperty().bindBidirectional(this.viewModel.discordStatusLabel());
        this.btn_logout.disableProperty().bindBidirectional(this.viewModel.logoutButtonEnabled());
        this.txt_plexAddress.textProperty().bindBidirectional(this.viewModel.plexAddressProperty());
        this.txt_plexPort.textProperty().bindBidirectional(this.viewModel.plexPortProperty());
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
        logViewWindow.show();
    }

    public void setLogViewStage(Stage logViewStage) {
        this.logViewWindow = logViewStage;
    }

    public void showSettings(ActionEvent actionEvent) {
        this.vbox_settings.setVisible(true);
        this.vbox_login.setVisible(false);
    }

    public void closeSettings(ActionEvent actionEvent) {
        this.vbox_settings.setVisible(false);
        this.vbox_login.setVisible(true);
    }

    public void chk_manualServer_onAction(ActionEvent actionEvent) {
        hbox_manualServerInput.setDisable(!chk_manualServer.isSelected());
    }
}


