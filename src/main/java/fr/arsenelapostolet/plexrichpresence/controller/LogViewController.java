package fr.arsenelapostolet.plexrichpresence.controller;

import fr.arsenelapostolet.plexrichpresence.SharedVariables;
import fr.arsenelapostolet.plexrichpresence.viewmodel.LogViewModel;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
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
    private ListView logs_list;

    @FXML
    public void initialize() {
        logs_list.setItems(SharedVariables.logList);
    }

    public void exportOnClick(ActionEvent e) {
        this.viewModel.exportLog();
    }

    public void clearLog(ActionEvent e) {
        this.viewModel.clearLogs();
    }
}
