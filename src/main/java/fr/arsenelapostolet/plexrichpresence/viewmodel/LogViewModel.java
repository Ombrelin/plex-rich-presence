package fr.arsenelapostolet.plexrichpresence.viewmodel;

import fr.arsenelapostolet.plexrichpresence.controller.LogViewController;
import javafx.stage.FileChooser;
import javafx.stage.Stage;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import java.io.File;
import java.io.FileWriter;

@Component
public class LogViewModel {

    private final Logger LOG = LoggerFactory.getLogger(LogViewModel.class);

    public void exportLog() {
        FileChooser fileChooser = new FileChooser();
        fileChooser.setTitle("Export log file");
        File exportFile = fileChooser.showSaveDialog(new Stage());

        if (exportFile == null) {
            return;
        }

        try {
            FileWriter writer = new FileWriter(exportFile.getAbsolutePath());
            for(String str: LogViewController.logList) {
                writer.write(str + System.lineSeparator());
            }
            writer.close();
            LOG.info("Log exported to " + exportFile.getAbsolutePath());
        } catch (Exception e) {
            LOG.error("Failed to export log.");
        }
    }

    public void clearLogs(){
        LogViewController.logList.clear();
    }
}
