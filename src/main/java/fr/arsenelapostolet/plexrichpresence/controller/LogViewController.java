package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.SharedVariables;
import fr.arsenelapostolet.plexrichpresence.viewmodel.LogViewModel;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.Button;
import javafx.scene.control.ListView;
import net.rgielen.fxweaver.core.FxmlView;
import org.springframework.stereotype.Component;

@Component
@FxmlView
public class LogViewController {
    private final LogViewModel viewModel;

    public LogViewController(LogViewModel viewModel) {
        this.viewModel = viewModel;
    }

    @FXML
    private Button btn_export;

    @FXML
    private ListView list_logs;

    @FXML
    public void initialize() {
        list_logs.setItems(SharedVariables.logList);
    }

    public void exportOnClick(ActionEvent e) {
        this.viewModel.exportLog();
    }


    public void clearLog(ActionEvent actionEvent) {
        SharedVariables.logList.clear();
    }
}
