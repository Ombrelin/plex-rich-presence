package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.ConfigManager;
import fr.arsenelapostolet.plexrichpresence.SharedVariables;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.Button;
import javafx.scene.control.CheckBox;
import javafx.scene.control.Label;
import javafx.scene.control.ListView;
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
        eventLog.setItems(SharedVariables.logList);

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
        logViewWindow.show();
    }

    public void setLogViewStage(Stage logViewStage) {
        this.logViewWindow = logViewStage;
    }

}


