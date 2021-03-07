package fr.arsenelapostolet.plexrichpresence.viewmodel;

import fr.arsenelapostolet.plexrichpresence.SharedVariables;
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
        File file = fileChooser.showSaveDialog(new Stage());

        if (file == null) {
            return;
        }

        try {
            FileWriter writer = new FileWriter(file.getAbsolutePath());
            for(String str: SharedVariables.logList) {
                writer.write(str + System.lineSeparator());
            }
            writer.close();
            LOG.info("Log exported to " + file.getAbsolutePath());
        } catch (Exception e) {
            LOG.error("Failed to export log.");
        }


    }
}
