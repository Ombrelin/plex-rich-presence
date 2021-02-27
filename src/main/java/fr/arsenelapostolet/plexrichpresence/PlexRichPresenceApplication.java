package fr.arsenelapostolet.plexrichpresence;

import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApiImpl;
import javafx.application.Application;
import javafx.application.Platform;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class PlexRichPresenceApplication {
	public static void main(String[] args) {
		Application.launch(PlexRefresherFX.class, args);
	}
}
