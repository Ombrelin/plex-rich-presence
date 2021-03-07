package fr.arsenelapostolet.plexrichpresence;

import fr.arsenelapostolet.plexrichpresence.controller.LogViewController;
import fr.arsenelapostolet.plexrichpresence.controller.MainController;
import fr.arsenelapostolet.plexrichpresence.controller.TrayIconController;
import javafx.application.Application;
import javafx.application.Platform;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;
import jfxtras.styles.jmetro.JMetro;
import jfxtras.styles.jmetro.Style;
import net.rgielen.fxweaver.core.FxControllerAndView;
import net.rgielen.fxweaver.core.FxWeaver;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.context.ConfigurableApplicationContext;

import javax.swing.*;
import java.awt.*;
import java.io.IOException;
import java.util.Properties;

public class PlexRefresherFX extends Application {

    private ConfigurableApplicationContext applicationContext;
    private final Logger LOG = LoggerFactory.getLogger(PlexRefresherFX.class);
    private Stage stage;
    private Stage logViewStage;
    private TrayIconController trayIconController;


    @Override
    public void init() {
        String[] args = getParameters().getRaw().toArray(new String[0]);
        Platform.setImplicitExit(false);
        this.applicationContext = new SpringApplicationBuilder()
                .sources(PlexRichPresenceApplication.class)
                .headless(false)
                .run(args);
    }

    @Override
    public void start(Stage stage) {
        this.stage = stage;
        trayIconController = new TrayIconController(stage);
        ConfigManager.initConfig();
        FxWeaver fxWeaver = applicationContext.getBean(FxWeaver.class);

        //Init main window
        FxControllerAndView mainController = fxWeaver.load(MainController.class);
        Parent root = (VBox) mainController.getView().orElseThrow();
        Scene scene = new Scene(root);
        JMetro jMetro = new JMetro(Style.DARK);
        jMetro.setScene(scene);
        scene.getStylesheets().add(getClass().getClassLoader().getResource("style.css").toExternalForm());
        scene.getStylesheets().add(getClass().getClassLoader().getResource("theme.css").toExternalForm());
        this.stage.setMinWidth(350);
        this.stage.setMinHeight(150);
        stage.setResizable(false);
        this.stage.getIcons().add(new Image(getClass().getClassLoader().getResourceAsStream("images/icon.png")));
        this.stage.setTitle(String.format("Plex Rich Presence v%s", getVersion()));
        this.stage.setScene(scene);

        // Only set on close/minimise event handlers if tray icon was initialized successfully.
        SwingUtilities.invokeLater(() -> {
            if (trayIconController.initTrayIcon()) {
                // On window close, minimise to tray.
                stage.setOnCloseRequest(event -> {
                    hideStage();
                });

                // On window minimise, minimise to tray.
                stage.iconifiedProperty().addListener((ov, t, t1) -> {
                    hideStage();
                });
            }
        });

        try {
            this.stage.show();
        } catch (Throwable e) {
            e.printStackTrace();
        }


        //Init log window
        try {
            logViewStage = new Stage();
            FxControllerAndView logViewController = fxWeaver.load(LogViewController.class);
            Parent logViewRoot = (VBox) logViewController.getView().orElseThrow();
            Scene logViewScene = new Scene(logViewRoot);
            JMetro jMetro2 = new JMetro(Style.DARK);
            jMetro2.setScene(logViewScene);
            logViewScene.getStylesheets().add(getClass().getClassLoader().getResource("style.css").toExternalForm());
            logViewScene.getStylesheets().add(getClass().getClassLoader().getResource("theme.css").toExternalForm());
            logViewStage.getIcons().add(new Image(getClass().getClassLoader().getResourceAsStream("images/icon.png")));
            logViewStage.setTitle("Logs");
            logViewStage.setScene(logViewScene);
        } catch (Throwable e) {
            e.printStackTrace();
        }


        ((MainController)mainController.getController()).setLogViewStage(logViewStage);

    }

    private String getVersion() {
        final Properties properties = new Properties();
        try {
            properties.load(getClass().getClassLoader().getResourceAsStream("application.properties"));
        } catch (IOException e) {
            e.printStackTrace();
        }
        return properties.getProperty("version");
    }

    private void hideStage() {
        stage.hide();
        new Thread(() -> {
            try {
                showSystemNotification("Plex Discord Rich Presence", "The application was minimized to tray. Open or exit the application via the tray icon.");
            } catch (IOException e) {
                LOG.error("Error showing system notification " + e.getMessage());
            }
        }).start();
    }

    @Override
    public void stop() {
        ConfigManager.saveConfig();
        this.applicationContext.close();
        Platform.exit();
        System.exit(0);
    }

    private void showSystemNotification(String title, String message) throws IOException {
        String os = System.getProperty("os.name");
        if (os.contains("Linux")) {
            ProcessBuilder builder = new ProcessBuilder(
                    "zenity",
                    "--notification",
                    "--title=" + title,
                    "--text=" + message);
            builder.inheritIO().start();
        } else if (os.contains("Mac")) {
            ProcessBuilder builder = new ProcessBuilder(
                    "osascript", "-e",
                    "display notification \"" + message + "\""
                            + " with title \"" + title + "\"");
            builder.inheritIO().start();
        } else if (SystemTray.isSupported()) {
            trayIconController.getTrayIcon().displayMessage(title, message, TrayIcon.MessageType.INFO);
        }
    }

}
