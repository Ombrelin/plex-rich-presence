package fr.arsenelapostolet.plexrichpresence;

import javafx.application.Application;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class PlexRichPresenceApplication {
    public static void main(String[] args) {
        if (System.getProperty("os.name").toLowerCase().contains("mac")) {
            // Hide dock icon on osx
            System.setProperty("apple.awt.UIElement", "true");
            java.awt.Toolkit.getDefaultToolkit();
        }

        Application.launch(PlexRefresherFX.class, args);
    }
}
