package fr.arsenelapostolet.plexrichpresence;

import ch.qos.logback.classic.Level;
import ch.qos.logback.classic.LoggerContext;
import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.application.Application;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class PlexRichPresenceApplication {
    public static void main(String[] args) {
        Application.launch(PlexRefresherFX.class, args);
    }
}
