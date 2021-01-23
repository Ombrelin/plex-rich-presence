package fr.arsenelapostolet.plexrichpresence;

import fr.arsenelapostolet.plexrichpresence.controller.MainController;
import javafx.application.Application;
import javafx.application.Platform;
import javafx.event.EventHandler;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.stage.Stage;
import javafx.stage.WindowEvent;
import jfxtras.styles.jmetro.JMetro;
import jfxtras.styles.jmetro.Style;
import net.rgielen.fxweaver.core.FxWeaver;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.context.ConfigurableApplicationContext;

public class PlexRefresherFX extends Application {

    private ConfigurableApplicationContext applicationContext;

    @Override
    public void init() {
        String[] args = getParameters().getRaw().toArray(new String[0]);

        this.applicationContext = new SpringApplicationBuilder()
                .sources(PlexRichPresenceApplication.class)
                .headless(false)
                .run(args);
    }

    @Override
    public void start(Stage stage) {
        ConfigManager.initConfig();
        FxWeaver fxWeaver = applicationContext.getBean(FxWeaver.class);
        Parent root = fxWeaver.loadView(MainController.class);
        Scene scene = new Scene(root);
        JMetro jMetro = new JMetro(Style.DARK);
        jMetro.setScene(scene);
        scene.getStylesheets().add(getClass().getClassLoader().getResource("style.css").toExternalForm());
        scene.getStylesheets().add(getClass().getClassLoader().getResource("theme.css").toExternalForm());
        stage.setMinWidth(300);
        stage.setMinHeight(200);
        stage.getIcons().add(new Image(getClass().getClassLoader().getResourceAsStream("images/icon.png")));
        stage.setTitle("Plex Rich Presence");
        stage.setScene(scene);
        stage.show();

    }

    @Override
    public void stop() {
        ConfigManager.saveConfig();
        this.applicationContext.close();
        Platform.exit();
        System.exit(0);
    }

}
